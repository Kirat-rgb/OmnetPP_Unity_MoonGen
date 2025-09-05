using UnityEngine;
using static TransmissionParameterManager;
public class LRWPANTransmissionManager : MonoBehaviour
{
    public LQI_Base lqiBase;
    public int lqi;
    public int targetId;
    public int GetLqi(TransmissionParameter parameter)
    {
        lqi = lqiBase.GetLqi(parameter);
        return lqi;
    }

    public int GetTargetId(TransmissionParameter parameter)
    {
        targetId = parameter.nodeB_Id;
        return targetId;
    }

    public UeBase GetTargetUe(TransmissionParameter parameter) 
    {
        return parameter.targetUeBase;
    }
}