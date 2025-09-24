using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FFDInfoDisplay : MonoBehaviour
{

    public TextMeshProUGUI ffdInfoDisplay;
    public FFDBase ffdBase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ffdInfoDisplay.text = "Ue_" + ffdBase.FFDId.ToString() + "\n"
                            + ffdBase.ulInfo + "\n"
                            + ffdBase.dlInfo;
    }
}
