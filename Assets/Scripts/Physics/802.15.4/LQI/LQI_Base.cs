using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static TransmissionParameterManager;

public class LQI_Base : MonoBehaviour
{
    public LQI_Computaion lqiComputation;
    public LteSINR lteSINR;
    public List<double> snrv = new List<double>();
    public int lqi;
    public double meanSNR;
    public int txmode = 2;
    //public UeBase ueBase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public int GetLqi(TransmissionParameter parameter)
    {
        snrv.Clear();
        snrv = lteSINR.GetSINR(true, parameter);
        meanSNR = lqiComputation.MeanSnr(snrv);
        lqi = lqiComputation.GetLqi(txmode, meanSNR);
        //lqi = cqiComputation.GetLqi(txmode, 32);
        return lqi;
    }
}