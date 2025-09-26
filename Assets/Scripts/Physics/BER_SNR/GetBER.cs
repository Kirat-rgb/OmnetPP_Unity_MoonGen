using System;
using UnityEngine;
using System.Collections.Generic;
using static TransmissionParameterManager;

public class GetBER : MonoBehaviour
{
    private double[] BERCurveSNR;
    private double[] BERCurveBER;
    public LteSINR lteSINR;
    public List<double> snrv = new List<double>();
    public double meanSNR;

    void Start()
    {
        BERCurveSNR = BerCurvesData.BerCurveSNR;
        BERCurveBER = BerCurvesData.BerCurveBER;
    }


    public double getBER(TransmissionParameter parameter)
    {
        snrv.Clear();
        snrv = lteSINR.GetSINR(true, parameter);
        meanSNR = MeanSnr(snrv);
        int closestSNR = 0;
        double difference = Math.Abs(meanSNR - BERCurveSNR[closestSNR]);
        for (int i = 1; i < BERCurveSNR.Length; i++)
        {
            double currentDifference = Math.Abs(meanSNR - BERCurveSNR[i]);
            if (currentDifference < difference)
            {
                closestSNR = i;
                difference = currentDifference;
            }
        }

        double BER = BERCurveBER[closestSNR];

        Debug.Log("Current meanSNR value: " + meanSNR);
        Debug.Log("Current BER value: " + BER);
        return BER;
    }
    
    public double MeanSnr(List<double> snr)
    {
        if (snr == null || snr.Count == 0)
            return 0;

        double sum = 0;
        foreach (double value in snr)
        {
            sum += value;
        }

        return sum / snr.Count;
    }



    /* double CatmullRom(double p0, double p1, double p2, double p3, double t)
    {
        double t2 = t * t;
        double t3 = t2 * t;
        return 0.5 * (
            (2.0 * p1) +
            (-p0 + p2) * t +
            (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3) * t2 +
            (-p0 + 3.0 * p1 - 3.0 * p2 + p3) * t3
        );
    } */

}
