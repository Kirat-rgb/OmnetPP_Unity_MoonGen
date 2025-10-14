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

    public double BERDisplay = 0;
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
        /* int closestSNR = 0;
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

        Debug.Log("Current meanSNR value: " + meanSNR); */

        double BER;
        double sum = 0.0;
        //meanSNR = -5;
        Debug.Log("meanSNR in dB: " + meanSNR);
        meanSNR = DeDB(meanSNR);
        Debug.Log("meanSNR: " + meanSNR);
        //Debug.Log("fakultät: " + 16 + " Ergebnis:" + faculty(16));
        for (int k = 2; k <= 16; k++)
        {
            double binomial = Faculty(16) / (Faculty(k) * Faculty(16 - k));
            sum = sum + Math.Pow(-1.0, k) * binomial * Math.Pow(Math.E, 20.0 * meanSNR * ((1.0 / k) - 1.0));
        }
        //Debug.Log("sum: " + sum);

        BER = 8.0 / 15.0 * (1.0 / 16.0) * sum;

        if (BER > 0.5 || BER < 0)
        {
            BER = 0.5;
        }

        Debug.Log("Current BER value: " + BER);

        BERDisplay = BER;

        return BER;
    }
    
    public double DeDB(double DBnumber)
    {
        double result = Math.Pow(10, DBnumber / 10);
        return result;
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

    public long Faculty(int number) {
        long result;
        
        if (number == 0 || number == 1)
        {
            return 1;
        }

        result = number;
        number = number - 1;
        while (number >= 1)
        {
            result = result * number;
            number = number - 1;
        }
        return result;
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
