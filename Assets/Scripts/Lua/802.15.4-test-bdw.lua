local mg      = require "moongen"
local memory  = require "memory"
local device  = require "device"
local ts      = require "timestamping"
local stats   = require "stats"
local log     = require "log"
local limiter = require "software-ratecontrol"
local pipe    = require "pipe"
local ffi     = require "ffi"
local libmoon = require "libmoon"
local histogram = require "histogram"
--local bit64   = require "bit64"

local namespaces = require "namespaces"
local socket = require("socket")


local PKT_SIZE	= 127 --Byte

function configure(parser)
	parser:description("Forward traffic between interfaces with moongen rate control")
	parser:option("-d --dev", "Devices to use, specify the same device twice to echo packets."):args(2):convert(tonumber)
	parser:option("-r --rate", "Forwarding rates in Mbps (two values for two links)"):args(2):convert(tonumber)
	parser:option("-t --threads", "Number of threads per forwarding direction using RSS."):args(1):convert(tonumber):default(1)
	parser:option("-l --latency", "Fixed emulated latency (in ms) on the link."):args(2):convert(tonumber):default({0,0})
	parser:option("-q --queuedepth", "Maximum number of packets to hold in the delay line"):args(2):convert(tonumber):default({0,0})
	return parser:parse()
end


function master(args)
	-- configure devices
	for i, dev in ipairs(args.dev) do
		args.dev[i] = device.config{
			port = dev,
			txQueues = args.threads,
			rxQueues = args.threads,
			rssQueues = 0,
			rssFunctions = {},
			--rxDescs = 4096,
			dropEnable = true,
			disableOffloads = true
		}
	end
	device.waitForLinks()

	-- print stats
	stats.startStatsTask{devices = args.dev}
	
	-- create the ring buffers
	-- should set the size here, based on the line speed and latency, and maybe desired queue depth
	local qdepth1 = args.queuedepth[1]
	--qdepth1 = math.ceil(2097152/1280)

	rate1 = args.rate[1] --its kbit/s not mbit/s
	rate2 = args.rate[2]


	if qdepth1 < 1 then
		qdepth1 = math.ceil((args.latency[1] * rate1 * 1000)/672)
		if (qdepth1 == 0) then
			qdepth1 = 1
		end

		print("automatically setting qdepth1="..qdepth1)
	end
	local qdepth2 = args.queuedepth[2]


	--qdepth2 = math.ceil(2097152/1280)


	if qdepth2 < 1 then
		qdepth2 = math.ceil((args.latency[2] * rate2 * 1000)/672)
		if (qdepth2 == 0) then
			qdepth2 = 1
		end
		print("automatically setting qdepth2="..qdepth2)
	end
	local ring1 = pipe:newPktsizedRing(qdepth1)
	local ring2 = pipe:newPktsizedRing(qdepth2)



	local ns = namespaces:get()



	-- start the forwarding tasks
	for i = 1, args.threads do
		mg.startTask("forward", ring1, args.dev[1]:getTxQueue(i - 1), args.dev[1], ns, rate1, args.latency[1],1)
		if args.dev[1] ~= args.dev[2] then
			mg.startTask("forward", ring2, args.dev[2]:getTxQueue(i - 1), args.dev[2], ns, rate2, args.latency[2],2)
		end
	end

	-- start the receiving/latency tasks
	for i = 1, args.threads do
		mg.startTask("receive", ring1, args.dev[2]:getRxQueue(i - 1), args.dev[2], ns, 1)
		if args.dev[1] ~= args.dev[2] then
			mg.startTask("receive", ring2, args.dev[1]:getRxQueue(i - 1), args.dev[1], ns, 2)
		end
	end

	mg.startTask("server", ns)

	mg.waitForTasks()
end


function receive(ring, rxQueue, rxDev, ns, threadId)
	--print("receive thread...")

	local bufs = memory.createBufArray()
	local count = 0
	local count_hist = histogram:new()
	local ringsize_hist = histogram:new()
	local ringbytes_hist = histogram:new()

	local tsc_hz = libmoon:getCyclesFrequency()
	local tsc_hz_ms = tsc_hz / 1000
	local overflow_count = 0
	local start_time = limiter:get_tsc_cycles() / tsc_hz_ms
	--local ring_capacity = pipe:capacityPktsizedRing(ring.ring)
	local ring_capacity = math.ceil(2097152/1280)


	while mg.running() do
		count = rxQueue:recv(bufs)
		count_hist:update(count)
		--print("receive thread count="..count)
		for iix=1,count do
			local buf = bufs[iix]
			local ts = limiter:get_tsc_cycles()
			buf.udata64 = ts
			--print("RXRX arrival: ", bit64.tohex(buf.udata64))
		end

		--Packet overflow handling
		if count > 0 then
			local ring_count = pipe:countPktsizedRing(ring.ring)
			if ring_count + count <= ring_capacity then
			   	--print("forwarding packet(s)")
				pipe:sendToPktsizedRing(ring.ring, bufs, count)
			else
				print("discarding packet(s)")
				overflow_count = overflow_count + count
				bufs:free(count)
			end
			--print("ring count: ",pipe:countPktsizedRing(ring.ring))
			ringsize_hist:update(pipe:countPktsizedRing(ring.ring))
		end

		if threadId == 2 then
			local current_time = limiter:get_tsc_cycles() / tsc_hz_ms
			--[[ ns.rlcMessage = string.format("[RLC] T: %s, Loss: %d, Queue: %d, of %d\n", 
                                              tostring(current_time - start_time), overflow_count or 0, pipe:countPktsizedRing(ring.ring) or 0, ring_capacity) ]]
			--print("RLC Loss: ",overflow_count)
		end

		

	end
	count_hist:print()
	count_hist:save("rxq-pkt-count-distribution-histogram-"..rxDev["id"]..".csv")
	ringsize_hist:print()
	ringsize_hist:save("rxq-ringsize-distribution-histogram-"..rxDev["id"]..".csv")
end


function forward(ring, txQueue, txDev, ns, rate, latency, threadId)

	local maxRetries = 3;

	print("forward with rate "..rate.." and latency "..latency.."")
	local numThreads = 1
	
	local linkspeed = txDev:getLinkStatus().speed
	print("linkspeed = "..linkspeed)

	local tsc_hz = libmoon:getCyclesFrequency()
	local tsc_hz_ms = tsc_hz / 1000
	print("tsc_hz = "..tsc_hz)

	local packetInfo = {}
	local packetInfoLength = 0

	ns.messageToSend = nil
	ns.messageId = 1
	ns.packetInfoLength = 0

	-- larger batch size is useful when sending it through a rate limiter
	local bufs = memory.createBufArray()  --memory:bufArray()  --(128)
	local count = 0

	local lastPrintTime = 0
	local start_time = limiter:get_tsc_cycles() / tsc_hz_ms

	local packetBuffer = {}
	local bufferIndex = 1

	while mg.running() do

		-- receive one or more packets from the queue
		count = pipe:recvFromPktsizedRing(ring.ring, bufs, 1)
		local sendCount = count

		for iix=1,count do

			senderEnergy = ns.energy_sender or 0
			interferenceEnergy = ns.energy_interference or 0
			senderLOS = ns.los_sender or false
			interferenceLOS = ns.los_interference or false
			interferenceTP = ns.interference_throughput or 0
			print("globals: "..tostring(senderLOS).." "..tostring(interferenceLOS).." "..interferenceTP.."")

			--varianz = math.random(-3, 3)
			varianz = 0
			--print("Varianz: "..varianz.."")
			if interferenceLOS then
				if senderLOS then
					if (interferenceEnergy > -20 and interferenceTP > 2.5) then
						rate = 0
					else
						rate = 38 + varianz
					end
				else
					rate = 0
				end
			else
				if senderLOS then
					rate = 38 + varianz
				else
					print("interferenceTP="..interferenceTP.."")
					rate = -2.8 * interferenceTP^2 - 3.5 * interferenceTP + 38 + varianz
					if interferenceTP > 2.5 then
						rate = 0
					end
				end
			end
			

			print("Rate: "..rate.."")

			if rate == 0 then
				loss = true;
				sendCount = sendCount - 1
				print("Packet lost")
			end

			local retransmissionAttempt = 0;
			local buf = bufs[iix]
			

			-- local current_time = limiter:get_tsc_cycles()
			-- get the buf's arrival timestamp and compute departure time
			local arrival_timestamp = buf.udata64
			--print("TXTX arrival: ", bit64.tohex(buf.udata64))


			local send_time = arrival_timestamp + (tsc_hz_ms * retransmissionAttempt)


			--print("TXTX send time: ", bit64.tohex(send_time))
			 --print("TXTX tsc_cycles: ", bit64.tohex(limiter:get_tsc_cycles()))

			-- spin/wait until it is time to send this frame
			-- this does not allow reordering of frames
			while limiter:get_tsc_cycles() < send_time do
				if not mg.running() then
					return
				end
			end

			local pktSize = buf.pkt_len + 24
			--print("TXTX set delay: ", (pktSize) * (linkspeed/rate - 1))
			buf:setDelay((pktSize) * (linkspeed*1000/rate - 1))


		end



		if count > 0 then
			-- the rate here doesn't affect the result afaict.  It's just to help decide the size of the bad pkts
			txQueue:sendWithDelayLoss(bufs, rate * numThreads, 0, sendCount)

			local currentSendTime = limiter:get_tsc_cycles() / tsc_hz_ms

			if threadId == 2 then
				for iix = 1, count do
				    sendCount = sendCount - 1
				    if sendCount < 0 then
				       currentSendTime = "Lost"
                    		    end
                		end
			end
			
		end

		local currentTime = os.clock()
	end
end


function server(ns)

	local tsc_hz = libmoon:getCyclesFrequency()
	local tsc_hz_ms = tsc_hz / 1000

    	local server = assert(socket.bind("127.0.0.1", 12345))
    	server:settimeout(0)
    	print("Server listening on 127.0.0.1:12345")

	local lastReportTime = limiter:get_tsc_cycles() / tsc_hz_ms
	local startTime = limiter:get_tsc_cycles() / tsc_hz_ms


	ns.packetIdToSend = 1

    	while mg.running() do
        local client = server:accept()
        if client then
	   client:settimeout(0)
	   local message, err = client:receive()
           if not err and message then
                print("Received raw message:", message)
                local ok, data = pcall(load("return " .. message))
		print("decoded message...")
                if ok and type(data) == "table" then
					--ns.bit_error_rate = data.bit_error_rate
					ns.energy_sender = data.energy_sender
					ns.energy_interference = data.energy_interference
					ns.los_sender = data.los_sender
					ns.los_interference = data.los_interference
					ns.interference_throughput = data.interference_throughput
					print("filled in ns table...")
                else
                    print("Invalid Lua table format.")
                end
                client:send("Processed: " .. message .. "\n")

            else
                print("Receive error:", err)
            end

            client:close()
        end

	mg.sleepMillis(1)  

        local currentTime = limiter:get_tsc_cycles() / tsc_hz_ms
        if currentTime > lastReportTime + 1000 then
            local report_client = socket.tcp()
            report_client:settimeout(0.1)

            if report_client:connect("127.0.0.1", 12350) and ns.rlcMessage then
                report_client:send(ns.rlcMessage)
            end

            report_client:close()
            lastReportTime = currentTime
        end

	mg.sleepMillis(1)
    end
    server:close()
    print("Server shut down.")
end