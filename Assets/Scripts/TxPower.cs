using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // �����ļ����������ռ�

public class TxPower : MonoBehaviour
{
    public LOS_Ray line;
    public double ffdPower = 26;
    public double ffdPowerFactor = 1;
    public double eNodeBtxPower = 40;
    public double eNodeBtxPowerFactor = 1;

    void Start()
    {
        
    }

    void Update()
    {
        if (line != null)
        {
            if (line.collision)
            {
                ffdPowerFactor = 0.05;
                eNodeBtxPowerFactor = 0.05;
            }
            else
            {
                ffdPowerFactor = 1;
                eNodeBtxPowerFactor = 1;
            }

            double result = ffdPower * ffdPowerFactor; 
            LogResultToFile(result); 
        }
    }

    void LogResultToFile(double result)
    {
        
    }
}