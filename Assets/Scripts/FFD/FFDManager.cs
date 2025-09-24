using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FFDManager : MonoBehaviour
{
    
    public string ulInfo = "";
    public string dlInfo = "";
    public string mobilityInfo = "";
    public FFDBase ffdSelect = null;

    private void Update()
    {
        ffdSelect = null;

        GameObject[] ffdObjects = GameObject.FindGameObjectsWithTag("FFD");
        if (ffdObjects != null)
        {
            ulInfo = "";
            dlInfo = "";
            mobilityInfo = "";
            foreach (GameObject ffdObject in ffdObjects)
            {
                FFDBase ffdBase = ffdObject.GetComponent<FFDBase>();

                if (ffdBase != null)
                {
                    ulInfo += ffdBase.ulInfo + "\n";
                    dlInfo += ffdBase.dlInfo + "\n";
                    mobilityInfo += ffdBase.mobilityInfo + "\n";
                    if (ffdBase.isSelect)
                    {
                        ffdSelect = ffdBase;
                    }
                }
                else
                {
                    Debug.LogWarning("No FFD component found on " + ffdObject.name);
                }
            }
        }
    }
}