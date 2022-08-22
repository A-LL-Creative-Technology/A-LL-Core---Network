using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PeerController : MonoBehaviour
{
    private static PeerController instance;

    public static PeerController GetInstance()
    {
        return instance;
    }

    public enum NetworkEntityType
    {
        CLIENT,
        SERVER,
        P2P
    }

    public NetworkEntityType entityType;

    //private readonly int CLIENT_LISTENING_PORT = 8901;
    private readonly int PEER_PORT = 8901; // default values

    public int ID;

    //[HideInInspector] public bool isPassivePeer = false;

    public string[] peersIP;
    // To Track
    [SerializeField] private GameObject arCameraToTrack;
    [SerializeField] private GameObject objectToVisualize;

    private Dictionary<string, GameObject> peersAvatarDictionary;

    // Network
    private int nbClients;

    private UDPConnection[] connections;


    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Init the peers
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        TryToGetIncomingMessages();

        SendUpdateToPeers();

        //UpdateDrawings();

    }

    private void Init()
    {
        // initialize the dictionary that will contain the other clients avatar
        peersAvatarDictionary = new Dictionary<string, GameObject>();

        nbClients = peersIP.GetLength(0);

        // Create the connections
        connections = new UDPConnection[nbClients];
        for (int i = 0; i < nbClients; i++)
        {
            connections[i] = new UDPConnection();
            connections[i].StartConnection(peersIP[i], PEER_PORT, PEER_PORT);

            Debug.Log("Created connection " + i + " (client " + peersIP[i] + ":" + PEER_PORT + "), server port " + PEER_PORT);
        }

    }

    //ONLY SERVER SIDE
    private void TryToGetIncomingMessages()
    {
        if (entityType == NetworkEntityType.CLIENT)
            return;
        // we check the messages on each connection
        for (int i = 0; i < nbClients; i++)
        {
            foreach (var rawMessage in connections[i].getMessages())
            {
                // parse the messsage
                Dictionary<string, float> messageDictionary = NetworkController.GetInstance().ParseIncomingMessage(rawMessage);

                VisualizePeers(messageDictionary);

                //InitPeersDrawings(messageDictionary);
            }
        }
    }

    //ONLY CLIENT SIDE
    private void SendUpdateToPeers()
    {
        // if it is a passive peer, it does not send anything over the network (similar to a server)
        if (entityType == NetworkEntityType.SERVER)
            return;

        Vector3 arCameraPosition = arCameraToTrack.transform.position;
        Quaternion arCameraRotation = arCameraToTrack.transform.rotation;

        string message = "id:" + ID;
        message += ":" + "camPos:" + arCameraPosition.x.ToString("F3") + "," + arCameraPosition.y.ToString("F3") + "," + arCameraPosition.z.ToString("F3");
        message += ":" + "camRot:" + arCameraRotation.x.ToString("F3") + "," + arCameraRotation.y.ToString("F3") + "," + arCameraRotation.z.ToString("F3") + "," + arCameraRotation.w.ToString("F3");

        for (int i = 0; i < nbClients; i++)
        {
            connections[i].Send(message);
        }
    }

    private void VisualizePeers(Dictionary<string, float> messageDictionary)
    {

        int peerID = (int)messageDictionary["clientID"];

        // if no instance yet for this client ID, then we create one
        string peerIDStr = peerID.ToString();

        if (!peersAvatarDictionary.ContainsKey(peerIDStr + "_cam"))
        {
            Debug.Log("Create new peer avatar " + peerID);

            peersAvatarDictionary.Add(peerIDStr + "_cam", objectToVisualize);
        }

        // update the peer avatars
        peersAvatarDictionary[peerIDStr + "_cam"].transform.localPosition = new Vector3(messageDictionary["camPosX"], messageDictionary["camPosY"], messageDictionary["camPosZ"]);
        peersAvatarDictionary[peerIDStr + "_cam"].transform.localRotation = new Quaternion(messageDictionary["camRotX"], messageDictionary["camRotY"], messageDictionary["camRotZ"], messageDictionary["camRotW"]);
    }

    void OnDestroy()
    {
        for (int i = 0; i < nbClients; i++)
        {
            connections[i].Stop();
        }
    }
}
