using UnityEngine;

public class ChannelModelManager : MonoBehaviour
{
    public FFDBase ffdBase;
    public GameObject transmission;
    private GameObject[] transmissionList;
    private TransmissionManager[] transmissionManagers;

    public int[] targetId;
    public FFDBase[] ueBases;

    void Start()
    {
        transmissionList = new GameObject[1];
        transmissionList[0] = transmission;
        transmissionManagers = new TransmissionManager[1];
        transmissionManagers[0] = transmission.GetComponent<TransmissionManager>();
    }

    public float timeSinceLastUpdate;
    private float lastUpdateTime;
    public float updateInterval = 0.1f;

    void Update()
    {
        timeSinceLastUpdate = Time.time - lastUpdateTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            CustomUpdate();
            lastUpdateTime = Time.time;
        }
    }

    void CustomUpdate()
    {
        int connectionNum = ffdBase.transmissionParameters.Length;

        //Debug.Log("connectionNum: " + connectionNum);

        if (connectionNum > transmissionList.Length)
        {
            System.Array.Resize(ref transmissionList, connectionNum);
            System.Array.Resize(ref transmissionManagers, connectionNum);  
        }

        for (int i = 0; i < connectionNum; i++)
        {
            if (transmissionList[i] == null)
            {
                transmissionList[i] = Instantiate(transmission, transmission.transform.parent);
                transmissionManagers[i] = transmissionList[i].GetComponent<TransmissionManager>();
            }
        }

        targetId = new int[connectionNum];
        ueBases = new FFDBase[connectionNum];
        for (int i = 0; i < connectionNum; i++)
        {
            targetId[i] = transmissionManagers[i].GetTargetId(ffdBase.transmissionParameters[i]);
            ueBases[i] = transmissionManagers[i].GetTargetUe(ffdBase.transmissionParameters[i]);
        }
    }
}