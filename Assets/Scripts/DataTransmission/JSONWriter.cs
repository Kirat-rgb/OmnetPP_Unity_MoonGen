using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class JSONWriter : MonoBehaviour
{
    [SerializeField] public string filename = "emulation_data.json";

    public float timeSinceLastUpdate;
    private float lastUpdateTime;
    private float updateInterval = 1f;

    /* public InputFieldManager inputHARQLossRate;
    public InputFieldManager inputPDCPThroughput; */
    //public InputFieldManager inputBERValue;

    public string jsonData;
    public bool writeJsonToLocal = true;
    public LteSINR SINRDevice2;
    public LteSINR SINRInterferenceCreator;
    public LOS_Ray LOSDevice2;
    public LOS_Ray LOSInterferenceCreator;

    void Update()
    {
        timeSinceLastUpdate = Time.time - lastUpdateTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            CustomUpdate();
            lastUpdateTime = Time.time;
        }
    }
    void CustomUpdate()
    {
        jsonData = WriteEmulationData();
    }

    public string WriteEmulationData()
    {
        EmulationData emulationData = new EmulationData
        {
            energy_sender = SINRInterferenceCreator.recvPower,
            energy_interference = SINRDevice2.recvPower,
            los_sender = LOSDevice2.collision,
            los_interference = LOSInterferenceCreator.collision
            //bit_error_rate = channelModelManager.ber
            /* HARQ_loss_rate = inputHARQLossRate.inputNumber,
            PDCP_throughput = inputPDCPThroughput.inputNumber */
        };

        string json = JsonConvert.SerializeObject(emulationData, Formatting.Indented);

        string filePath = Path.Combine(Application.streamingAssetsPath, filename);

        if (writeJsonToLocal)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(json);
            }
        }

        return json;
    }
}

[System.Serializable]
public class EmulationData
{
    //public double bit_error_rate;
    public double energy_interference;
    public double energy_sender;
    public bool los_interference;
    public bool los_sender;
}