using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviour
{

    private static NetworkController instance;

    public static NetworkController GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public Dictionary<string, float> ParseIncomingMessage(string rawMessage)
    {
        // split the rawMessage
        string[] splitMessage = rawMessage.Split(':');

        Dictionary<string, float> messageDictionary = new Dictionary<string, float>();

        // parse the client ID
        if (splitMessage[0] != "id")
        {
            Debug.Log("Message is not in the correct format (no ID): " + rawMessage);
            return null;
        }

        messageDictionary.Add("clientID", int.Parse(splitMessage[1]));

        // parse the head position
        if (splitMessage[2] != "camPos")
        {
            Debug.Log("Message is not in the correct format (no camera position): " + rawMessage);
            return null;
        }

        string[] cameraPositions = splitMessage[3].Split(',');
        messageDictionary.Add("camPosX", float.Parse(cameraPositions[0].Trim()));
        messageDictionary.Add("camPosY", float.Parse(cameraPositions[1].Trim()));
        messageDictionary.Add("camPosZ", float.Parse(cameraPositions[2].Trim()));


        // parse the head position
        if (splitMessage[4] != "camRot")
        {
            Debug.Log("Message is not in the correct format (no camera rotation): " + rawMessage);
            return null;
        }

        string[] cameraRotations = splitMessage[5].Split(',');
        messageDictionary.Add("camRotX", float.Parse(cameraRotations[0].Trim()));
        messageDictionary.Add("camRotY", float.Parse(cameraRotations[1].Trim()));
        messageDictionary.Add("camRotZ", float.Parse(cameraRotations[2].Trim()));
        messageDictionary.Add("camRotW", float.Parse(cameraRotations[3].Trim()));


        //// parse the drawing progress
        //if (splitMessage[8] != "dr")
        //{
        //    Debug.Log("Message is not in the correct format (no drawing): " + rawMessage);
        //    return null;
        //}

        //messageDictionary.Add("isDrawingInProgress", int.Parse(splitMessage[9]));

        return messageDictionary;
    }


}
