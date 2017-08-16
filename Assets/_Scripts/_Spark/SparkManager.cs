using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

/// <summary>
/// Spark Manager Handles all REST calls between the app and the Spark API
/// </summary>
/// 

#region Json Data Containers

[System.Serializable]
public class SparkMessageItems
{
    public SparkMessage[] items;
}

[System.Serializable]
public class SparkMessage
{
    public string id;
    public string roomId;
    public string roomType;
    public string text;
    public string[] files;
    public string personId;
    public string personEmail;
    public string created;
}

[System.Serializable]
public class PersonItems
{
    public Person[] items;
}

[System.Serializable]
public class Person
{
    public string id;
    public string[] emails;
    public string displayName;
    public string nickName;
    public string firstNAme;
    public string lastName;
    public string avatar;
    public string orgId;
    public string[] roles;
    public string[] licenses;
    public string created;
    public string lastActivity;
    public string status;
    public bool invitePending;
    public bool loginEnabled;
    public string type;
}

[System.Serializable]
public class RoomItems
{
    public Room[] items;
}

[System.Serializable]
public class Room
{
    public string id;
    public string title;
    public string type;
    public bool isLocked;
    public string lastActivity;
    public string creatorId;
    public string Created;
}

[System.Serializable]
public class MembershipItems
{
    public Membership[] items;
}

[System.Serializable]
public class Membership
{
    public string id;
    public string roomId;
    public string personId;
    public string personEmail;
    public string personDisplayName;
    public string personOrgId;
    public bool isModerator;
    public bool isMonitor;
    public string created;
}


#endregion

public class SparkManager : MonoBehaviour {

    public enum RequestApiType
    {
        Rooms,
        Memberships,
        Messages,
        People,
        Webhooks,
        Me
    }

    private static SparkManager _instance;

    public static SparkManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<SparkManager>();
            }

            return _instance;
        }
    }

    //private string hookUrl = "http://mki.io/app/";
    private string localPersonID;
    public string token = "";
    private string currentRoomID = "Y2lzY29zcGFyazovL3VzL1JPT00vOGE4OWZjNTAtM2RhMC0xMWU3LTkyYTQtMDE0MzhmYTI0YWNh"; //this is hardcoded for development.

    public UnityEvent OnChangedRoom = new UnityEvent();
    public UnityEvent OnAuthenticated = new UnityEvent();

    private void Awake ()
    {
        token = PlayerPrefs.GetString("SparkToken", "");
    }

    #region System Events

    public void ChangeRoom(string newRoomID)
    {
        currentRoomID = newRoomID;

        if(OnChangedRoom != null) OnChangedRoom.Invoke();
    }

    public void UpdateToken(string _token)
    {
        token = _token;
        PlayerPrefs.SetString("SparkToken", token);
        Debug.Log("Access Token Updated :" + token);

        if (OnAuthenticated != null) OnAuthenticated.Invoke();
    }

    #endregion  

    #region Spark Actions

    public void CreateRoom(string roomName)
    {
        string json = "{\"title\": \"" + roomName + "\" }";
        StartCoroutine(PostRequest(RequestApiType.Rooms, json, RoomCreated));
    }

    public void GetRooms(System.Action<string> sendTo)
    {
        StartCoroutine(GetRequest(RequestApiType.Rooms, "", sendTo));
    }


    public void InviteMember(string userId)
    {
        InviteMember(currentRoomID, userId);
    }
       
    public void InviteMember(string roomId, string userId)
    {
        string json = "{\"roomId\": \"" + roomId + "\", \"personEmail\" : \"" + userId + "\", \"isModerator\" : false }";
        Debug.Log(json);
        StartCoroutine(PostRequest(RequestApiType.Memberships, json, InviteComplete));
    }


    //Send Pure Text Message
    public void SendTextMessage(string message)
    {
        SendMessage(currentRoomID, message);
    }

    public void SendTextMessage(string roomId, string message)
    {
        string json = "{\"roomId\": \"" + roomId + "\", \"text\" : \"" + message + "\"}";
        Debug.Log(json);
        StartCoroutine(PostRequest(RequestApiType.Messages, json, SendComplete));
    }


    //Send Text Message with URL attachment
    public void SendMessageAttachmentURL(string message, string fileURL)
    {
        SendMessageAttachmentURL(currentRoomID, message, fileURL);
    }

    public void SendMessageAttachmentURL(string roomId, string message, string fileURL)
    {
        string json = "{\"roomId\": \"" + roomId + "\",  \"file\" : \"" + fileURL + "\", \"text\" : \"" + message + "\"}";
        Debug.Log(json);
        StartCoroutine(PostRequest(RequestApiType.Messages, json, SendComplete));
    }


    //Send URL attachment only
    public void SendAttachmentURL(string fileURL)
    {
        SendAttachmentURL(currentRoomID, fileURL);
    }

    public void SendAttachmentURL(string roomId, string fileURL)
    {
        string json = "{\"roomId\": \"" + roomId + "\",  \"file\" : \"" + fileURL + "\"}";
        StartCoroutine(PostRequest(RequestApiType.Messages, json, SendComplete));
    }


    //Send Local File -- Different POST call for the binary file, uses forms instead
    public void SendLocalFile(byte[] file)
    {
        SendLocalFile(currentRoomID, file);
    }

    public void SendLocalFile(string roomId, byte[] file)
    {
        StartCoroutine(PostFormRequest(RequestApiType.Messages, roomId, file, SendComplete));
    }


    public void GetPerson(string personId, string personEmail, System.Action<string> sendTo = null)
    {
        string requestUrl = "?personId=" + personId + "&email=" + personEmail;

        StartCoroutine(GetRequest(RequestApiType.People, requestUrl, sendTo));
    }


    public void GetMe( System.Action<string> sendTo = null)
    {
        StartCoroutine(GetRequest(RequestApiType.Me, "", sendTo));
    }

    //Retreive messages for room
    public void GetMessages(string roomId = "", System.Action<string> sendTo = null)
    {
        if(roomId == "" )
        {
            roomId = "?roomId=" + currentRoomID;
        }
        else
        {
            roomId = "?roomId=" + roomId;
        }

        StartCoroutine(GetRequest(RequestApiType.Messages, roomId, sendTo));
    }

    public void GetMembers(string roomId = "", System.Action<string> sendTo = null)
    {
        if (roomId == "")
        {
            roomId = "?roomId=" + currentRoomID;
        }
        else
        {
            roomId = "?roomId=" + roomId;
        }

        StartCoroutine(GetRequest(RequestApiType.Memberships, roomId, sendTo));
    }

    //This was for the Webhook stuff I never finished
    //public void SubscribeToRoom( string roomId)
    //{
    //    string json = "{ \"name\":\"Sub-" + roomId + "\","+
    //            "\"targetUrl\":\"" + hookUrl + "\"," +
    //            "\"resource\":\"messages\"," +
    //            "\"filter\":\"roomId=" + roomId + "\"}";    


    //}

    #endregion

    #region Callbacks

    private void SendComplete()
    {

    }

    private void InviteComplete()
    {

    }

    private void RoomCreated()
    {

    }

    #endregion

    #region REST Requests

    private IEnumerator PostRequest(RequestApiType requestApiType, string json, System.Action OnComplete)
    {
        byte[] raw = Encoding.UTF8.GetBytes(json); //encode the body data

        //Create the request
        var www = new UnityWebRequest(GetAPIAddress(requestApiType), "POST");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(raw); //hmm can probability unify these. 
        www.uploadHandler.contentType = "application/json";
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + token);
        www.SetRequestHeader("Content-Type", "application/json");

        //Away we go. 
        yield return www.Send();

        Debug.Log("Status Code: " + www.responseCode);

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }


    //this is currently used to post screenshots directly from inside Unity.
    private IEnumerator PostFormRequest(RequestApiType requestApiType, string roomId, byte[] bytes, System.Action OnComplete = null)
    {
        WWWForm newForm = new WWWForm();
        newForm.AddField("roomId", roomId);
        newForm.AddBinaryData("files", bytes);

        UnityWebRequest www = UnityWebRequest.Post(GetAPIAddress(requestApiType), newForm);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.Send();

        Debug.Log("Status Code: " + www.responseCode);

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }

    IEnumerator GetRequest(RequestApiType requestApiType, string requestParameters = "", System.Action<string> OnGetResults = null)
    {
        UnityWebRequest www = UnityWebRequest.Get(GetAPIAddress(requestApiType) + requestParameters);
        www.SetRequestHeader("Authorization", "Bearer " + token);

        Debug.Log("Sending GetRequest for " + requestApiType);
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            if(OnGetResults != null)
            {
                OnGetResults.Invoke(www.downloadHandler.text);
            }
              
            // Or retrieve results as binary data
            //byte[] results = www.downloadHandler.data;
        }
    }

    #endregion

    #region Utility 
    private string GetAPIAddress(RequestApiType requestApiType)
    {
        switch(requestApiType)
        {
            case RequestApiType.Memberships:
                return "https://api.ciscospark.com/v1/memberships";
            case RequestApiType.Rooms:
                return "https://api.ciscospark.com/v1/rooms";
            case RequestApiType.Messages:
                return "https://api.ciscospark.com/v1/messages";
            case RequestApiType.People:
                return "https://api.ciscospark.com/v1/people";
            case RequestApiType.Webhooks:
                return "https://api.ciscospark.com/v1/webhooks";
            case RequestApiType.Me:
                return "https://api.ciscospark.com/v1/people/me";
            default:
                return "";
        }
    }



    public bool IsAuthenticated()
    {
        //this should eventually contain checks for expired tokens, refresh tokens, etc.
        //for now it just checks that we actually have a token
        return (token != "");
    }

    #endregion

}
