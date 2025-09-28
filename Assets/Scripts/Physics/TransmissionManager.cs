using UnityEngine;
using static TransmissionParameterManager;
public class TransmissionManager : MonoBehaviour
{
    public int targetId;
    public GetBER getber;
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
        ber = getber.getBER(parameter);
        return ber;
    }
}