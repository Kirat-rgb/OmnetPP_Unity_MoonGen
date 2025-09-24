using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // �����ļ����������ռ�

public class TxPower : MonoBehaviour
{
    public LOS_Ray line;
    public double uePower = 26;
    public double uePowerFactor = 1;
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
                uePowerFactor = 0.05;
                eNodeBtxPowerFactor = 0.05;
            }
            else
            {
                uePowerFactor = 1;
                eNodeBtxPowerFactor = 1;
            }

            double result = uePower * uePowerFactor; 
            //LogResultToFile(result); 
        }
    }

    /* void LogResultToFile(double result)
    {
        
    } */
}