using TMPro;
using UnityEngine;
using System;



public class UINodeSelectInfo : MonoBehaviour
{
    public FFDManager ffdManager;
    private FFDBase selectFFD;
    public string displayInfo;
    public TextMeshProUGUI nodeInfoDisplay;
    public GameObject display;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        selectFFD = null;
        displayInfo = string.Empty;
        display.SetActive(false);
        if (ffdManager.ffdSelect != null) 
        {
            display.SetActive(true);
            selectFFD = ffdManager.ffdSelect;
            displayInfo =
                            "<b>Nodetype:</b> Ue\n" +
                            "<b>NodeId:</b> " + selectFFD.FFDId + "\n" +
                            "<b>Position:</b> " +
                                Math.Round(selectFFD.prefabPosition.x, 1) + " " +
                                -Math.Round(selectFFD.prefabPosition.z, 1) + " " +
                                Math.Round(selectFFD.prefabPosition.y, 1) + "\n" +
                            "<b>TxPower:</b> " + Math.Round(selectFFD.txPowerUl, 1) + "dBm" + "\n" + 
                            "<b>Upload:</b> " + selectFFD.FFDId + "->" + selectFFD.TargetEnb.enbId +
                                " TxPower: " + Math.Round(selectFFD.txPowerUl, 1) + "dBm" + " Loss: " + Math.Round(selectFFD.LOS_Ray.GetLosLoss(true, selectFFD.transmissionParameters[0]),1) + "dBm" + "\n" +
                            "<b>Download:</b> " + selectFFD.TargetEnb.enbId + "->" + selectFFD.FFDId +
                                " TxPower: " + Math.Round(selectFFD.txPowerDl, 1) + "dBm" + " Loss " + Math.Round(selectFFD.LOS_Ray.GetLosLoss(false, selectFFD.transmissionParameters[0]), 1) + "dBm" + "\n";
            nodeInfoDisplay.text = displayInfo;
        }
    }
}
