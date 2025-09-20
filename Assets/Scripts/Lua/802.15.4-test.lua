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
local namespaces = require "namespaces"
local socket = require("socket")


local PKT_SIZE	= 60

function configure(parser) --? -J
	parser:description("Forward traffic between interfaces with moongen rate control")
	parser:option("-d --dev", "Devices to use, specify the same device twice to echo packets."):args(2):convert(tonumber)
	parser:option("-r --rate", "Forwarding rates in Mbps (two values for two links)"):args(2):convert(tonumber)
	parser:option("-t --threads", "Number of threads per forwarding direction using RSS."):args(1):convert(tonumber):default(1)
	parser:option("-l --latency", "Fixed emulated latency (in ms) on the link."):args(2):convert(tonumber):default({0,0})
	parser:option("-q --queuedepth", "Maximum number of packets to hold in the delay line"):args(2):convert(tonumber):default({0,0})
	parser:option("-o --loss", "Rate of packet drops"):args(2):convert(tonumber):default({0,0})
	parser:option("-H --harq", "HARQ loss rate on the link."):args(2):convert(tonumber):default({0,0})
	return parser:parse()
end


function master(args)
	
end


function receive()

end


function forward()

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
                if ok and type(data) == "table" then
                    --print("Parsed table:", data)
					--print("Old HARQ_loss_rate:", ns.HARQ_loss_rate)
					ns.HARQ_loss_rate = data.HARQ_loss_rate
					--print("New HARQ_loss_rate:", ns.HARQ_loss_rate)
					ns.PDCP_throughput = data.PDCP_throughput
					print("New PDCP_throughput:", ns.PDCP_throughput)
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

		
	local success, test_err = pcall(function()
        while ns.packetInfoLength and ns.packetIdToSend <= ns.packetInfoLength do
            if ns.messageToSend and ns.messageId >= ns.packetIdToSend then
                local send_client = socket.tcp()
                send_client:settimeout(0.1)

                if send_client:connect("127.0.0.1", 12350) then
                    send_client:send(ns.messageToSend)
                    ns.packetIdToSend = ns.messageId + 1    
                end
                send_client:close()
            end
        end
    end)

    if not success then
        print("Error sending message: ", test_err)
    end

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


