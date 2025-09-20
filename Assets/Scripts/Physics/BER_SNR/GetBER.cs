using System;
using UnityEngine;

public  class GetBER : MonoBehaviour
{
    private double[,] BERCurve;

    void Start()
    {
        BERCurve = Data.BerCurve;
    }


    public double GetBER()
    {
        SNR = lteSINR.GetSINR(true, parameter);
        closestSNR = BERCurve[0];
        int difference = Math.Abs(number - closest);
        for (int i = 1; i < BERCurve.Count; i++)
        {
            int currentDifference = Math.Abs(number - list[i]);
            if (currentDifference < difference)
            {
                closestSNR = BERCurve[i];
                difference = currentDifference;
            }
        }




        return BER;
    }



    double CatmullRom(double p0, double p1, double p2, double p3, double t)
    {
        double t2 = t * t;
        double t3 = t2 * t;
        return 0.5 * (
            (2.0 * p1) +
            (-p0 + p2) * t +
            (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3) * t2 +
            (-p0 + 3.0 * p1 - 3.0 * p2 + p3) * t3
        );
    }

}
