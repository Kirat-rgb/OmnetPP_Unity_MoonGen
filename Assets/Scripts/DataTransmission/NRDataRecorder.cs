using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using TMPro;

[System.Serializable]
public class NRDataRecord
{
    public float t;
    public double b;

    public NRDataRecord(float time, double ber)
    {
        t = time;
        b = ber;
    }
}

public class NRDataRecorder : MonoBehaviour
{
    public float recordInterval = 0.1f;  
    public float recordDuration = 10.0f; 
    public float elapsedTime = 0.0f;    
    private bool isRecording = false;

    private List<NRDataRecord> recordedData = new List<NRDataRecord>();
    public UeBase ueBase;
    public TransmissionManager transmissionManager;

    public InputFieldManager intervalInput;
    public InputFieldManager durationInput;

    public TMP_Text showElapesdTime;
    public TMP_Text showPkts;
    public TMP_Text showPER;
    public TMP_Text showITBS;

    public ConvertJsonToLua jsonToLua;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isRecording)
        {
            StartCoroutine(RecordData());
        }


        showElapesdTime.text = elapsedTime.ToString("000.0");
        recordInterval = intervalInput.inputNumber;
        recordDuration = durationInput.inputNumber;
        
    }

    IEnumerator RecordData()
    {
        isRecording = true;
        recordedData.Clear();
        float startTime = Time.time;

        while (Time.time - startTime < recordDuration)
        {
            elapsedTime = Time.time - startTime;
            recordedData.Add(new NRDataRecord(elapsedTime, transmissionManager.GetBER(ueBase.transmissionParameters[0])));

            yield return new WaitForSeconds(recordInterval);
        }

        Debug.Log("Recording finished!");

        SaveToJson();
        isRecording = false;
    }


    void SaveToJson()
    {
        string json = JsonConvert.SerializeObject(recordedData, Formatting.Indented);
        string filePath = Path.Combine(Application.streamingAssetsPath, "emulation_data_record.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, json);
        Debug.Log("Data saved to: " + filePath);
        jsonToLua.WriteLua(json, "emulation_data_record.lua");
    }
}