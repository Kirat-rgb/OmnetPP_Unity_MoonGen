using UnityEngine;

public class ChannelModelManager : MonoBehaviour
{
    public UeBase ueBase;
    public GameObject transmission;
    private GameObject[] transmissionList;
    private TransmissionManager[] transmissionManagers;

    public int[] targetId;
    public UeBase[] ueBases;

    public double ber;

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
        int connectionNum = ueBase.transmissionParameters.Length;

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
        ueBases = new UeBase[connectionNum];
        for (int i = 0; i < connectionNum; i++)
        {
            ber = transmissionManagers[i].GetBER(ueBase.transmissionParameters[i]);
            targetId[i] = transmissionManagers[i].GetTargetId(ueBase.transmissionParameters[i]);
            ueBases[i] = transmissionManagers[i].GetTargetUe(ueBase.transmissionParameters[i]);
        }
    }
}