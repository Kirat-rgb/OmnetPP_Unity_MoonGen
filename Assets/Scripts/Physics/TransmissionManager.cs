using UnityEngine;
using static TransmissionParameterManager;
public class TransmissionManager : MonoBehaviour
{
    public int targetId;
    public GetBER getBER;
    public double ber;

    public int GetTargetId(TransmissionParameter parameter)
    {
        targetId = parameter.nodeB_Id;
        return targetId;
    }

    public UeBase GetTargetUe(TransmissionParameter parameter)
    {
        return parameter.targetUeBase;
    }

    public double GetBER(TransmissionParameter parameter)
    {
        ber = getBER.getBER(parameter);
        return ber;
    }
}