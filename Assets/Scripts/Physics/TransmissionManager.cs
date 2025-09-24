using UnityEngine;
using static TransmissionParameterManager;
public class TransmissionManager : MonoBehaviour
{
    public int targetId;

    public int GetTargetId(TransmissionParameter parameter)
    {
        targetId = parameter.nodeB_Id;
        return targetId;
    }

    public FFDBase GetTargetUe(TransmissionParameter parameter) 
    {
        return parameter.targetFFDBase;
    }
}
