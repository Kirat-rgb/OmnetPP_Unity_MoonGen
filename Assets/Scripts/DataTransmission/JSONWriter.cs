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
    public ChannelModelManager channelModelManager;

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
            bit_error_rate = channelModelManager.ber
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
    public double bit_error_rate;
}