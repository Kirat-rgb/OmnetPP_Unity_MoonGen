using UnityEngine;
using System;
using static TransmissionParameterManager;
using UnityEngine.UIElements;
using System.Linq;



public class FFDBase : MonoBehaviour
{

    public LOS_Ray LOS_Ray;
    public double txPowerUl = 0.0f;
    public double txPowerDl = 0.0f;
    public int FFDId = 0;


    public enum FFDType
    {
        Standard,
        FFDD2D
    }
    public FFDType ffdType = FFDType.Standard;
    public FFDParameters ffdParameters;

    private bool losCollision = false;

    public UeStatusDisplay statusDisplay;
    public bool isSelect = false;

    public EnbInfo TargetEnb;

    public UeMovementControl prefabMovement;
    public Vector3 prefabPosition;

    public string mobilityInfo;

    public string ulInfo = "";
    public string dlInfo = "";

    public GameObject ffdObject;

    public TransmissionParameter[] transmissionParameters;
    public FFDBase[] ffdBases;

    public float maxD2D_ConnectionDistance = 300;
    public int maxD2D_ConnectionNum = 4;

    void Start()
    {
        //txPowerBaseUl = UnityEngine.Random.Range( 25.0f, 27.0f );
        FindOtherFFDBases();
    }


    void Update()
    {
        switch (ffdType)
        {
            case FFDType.Standard:
                
                transmissionParameters = new TransmissionParameter[1];
                transmissionParameters[0] = new TransmissionParameter();
                updateTransmissionParameterStandard(TargetEnb);
                break;


            case FFDType.FFDD2D:
                FindOtherFFDBases();
                transmissionParameters = new TransmissionParameter[ffdBases.Length];
                for (int i = 0; i < transmissionParameters.Length; i++)
                {
                    transmissionParameters[i] = new TransmissionParameter();
                }
                updateTransmissionParameterD2D(ffdBases);
                break;
        }

        if (prefabMovement != null)
        {
            prefabPosition = prefabMovement.positionLocal;
            mobilityInfo = FFDId.ToString() + ": " + Math.Round(prefabPosition.x, 1) + " " + -Math.Round(prefabPosition.z, 1) + " " + Math.Round(prefabPosition.y, 1);
        }

        if (statusDisplay != null) isSelect = statusDisplay.status;


    }

    void updateTransmissionParameterD2D(FFDBase[] ueBases)
    {
        for (int i = 0; i < transmissionParameters.Length; i++)
        {
            transmissionParameters[i].nodeA_Id = FFDId;
            transmissionParameters[i].positionA = ffdObject.transform.position;
            transmissionParameters[i].txPowerA = ffdParameters.txPower;
            transmissionParameters[i].antennaGainA = ffdParameters.antennaGain;

            transmissionParameters[i].nodeB_Id = ueBases[i].FFDId;
            transmissionParameters[i].positionB = ueBases[i].ffdObject.transform.position;
            transmissionParameters[i].txPowerB = ueBases[i].ffdParameters.txPower;
            transmissionParameters[i].antennaGainB = ueBases[i].ffdParameters.antennaGain;

            transmissionParameters[i].frequency = ffdParameters.frequency;
            transmissionParameters[i].numBands = ffdParameters.numBands;
            transmissionParameters[i].numLayers = ffdParameters.numLayers;
            transmissionParameters[i].cableLoss = ffdParameters.cableLoss;
            transmissionParameters[i].noiseFigure = ffdParameters.noiseFigure;
            transmissionParameters[i].thermalNoise = ffdParameters.thermalNoise;

            transmissionParameters[i].lastUpdateTime = Time.time;
            transmissionParameters[i].targetFFDBase = ffdBases[i];
        }
    }

    void updateTransmissionParameterStandard(EnbInfo enb)
    {
            transmissionParameters[0].nodeA_Id = FFDId;
            transmissionParameters[0].positionA = ffdObject.transform.position;
            transmissionParameters[0].txPowerA = ffdParameters.txPower;
            transmissionParameters[0].antennaGainA = ffdParameters.antennaGain;

            transmissionParameters[0].nodeB_Id = enb.enbId;
            transmissionParameters[0].positionB = enb.enbObject.transform.position;
            transmissionParameters[0].txPowerB = enb.txPower;
            transmissionParameters[0].antennaGainB = enb.antennaGain;

            transmissionParameters[0].frequency = ffdParameters.frequency;
            transmissionParameters[0].numBands = ffdParameters.numBands;
            transmissionParameters[0].numLayers = ffdParameters.numLayers;
            transmissionParameters[0].cableLoss = ffdParameters.cableLoss;
            transmissionParameters[0].noiseFigure = ffdParameters.noiseFigure;
            transmissionParameters[0].thermalNoise = ffdParameters.thermalNoise;

        transmissionParameters[0].lastUpdateTime = Time.time;
    }

    void FindOtherFFDBases()
    {
        FFDBase[] allFFDBases = FindObjectsByType<FFDBase>(FindObjectsSortMode.None);
        ffdBases = allFFDBases
            .Where(ffd => ffd != this && Vector3.Distance(ffd.ffdObject.transform.position, this.ffdObject.transform.position) < maxD2D_ConnectionDistance)
            .Take(maxD2D_ConnectionNum)
            .ToArray();
    }
}
