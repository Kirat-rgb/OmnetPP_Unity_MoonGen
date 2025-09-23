using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; 
using TMPro;

public class DataDisplay : MonoBehaviour
{
    public TextMeshProUGUI currentBER;
    public TextMeshProUGUI throughput;

    public float timeSinceLastUpdate;
    private float lastUpdateTime;
    public float updateInterval = 0.2f;


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
        
    }
}