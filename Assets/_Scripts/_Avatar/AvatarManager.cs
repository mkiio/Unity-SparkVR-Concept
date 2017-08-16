using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarManager : MonoBehaviour {

    public List<Avatar> avatars = new List<Avatar>();
    public GameObject avatarPrefab;
    public string lastMessageID = "";
    public SparkMessageItems sparkMessageItems; //purely for debug at the moment, so I can see what the last update was in the inspector.
    public float messageUpdateDelay = 120f;

    private bool runMessageCycle = true; 
    private bool updateAll = false; //this is to make the avatar update grab all avatars, like when first entering a room
    private int currentAvatar = 0; //this tracks the avatar currently being updated in an all avatars update. 

    private AvatarSpawnPoint[] avatarSpawnPoints;

    void Start ()
    {
        SparkManager.instance.OnChangedRoom.AddListener(OnChangedRoom);

        avatarSpawnPoints = FindObjectsOfType<AvatarSpawnPoint>();
    }

    #region Avatar Cleanup and Room Change

    private void OnChangedRoom()
    {
        StartCoroutine(CleanUpRoom());
    }
    //This handles when a user leaves the room
    //We need to remove all the avatars in the room they left, then respawn them in the new room.
    private IEnumerator CleanUpRoom()
    {
        CleanUpAvatars();

        //Waiting for the next frame to kick off the spawn cycle
        yield return null;
        
        GetRoomAvatars();
    }

    private void CleanUpAvatars()
    {
        Debug.Log("Cleaning up Avatars");

        runMessageCycle = false; //disable the update message cycle for the room

        foreach (Avatar avatar in avatars)
        {
            Destroy(avatar.gameObject);
        }

        avatars.Clear();

        for (int i = 0; i < avatarSpawnPoints.Length; i++) //we reset the spawn points so the new room can use them
        {
            avatarSpawnPoints[i].isOccupied = false;
        }

        currentAvatar = 0;  //reset the udpate counter for the innitial grab
        updateAll = false;

        lastMessageID = "";
    }

    #endregion

    #region Avatar Message Updates

    //this retreives the messages from the messages API on a regular interval
    private IEnumerator UpdateMessageCycle()
    {
         while(runMessageCycle)
        { 
            yield return new WaitForSeconds(messageUpdateDelay);
            SparkManager.instance.GetMessages("", ParseMessages);
        }
    }

    //Callback from GET Request, throws the results into an object and updates the avatar messages
    public void ParseMessages(string json)
    {
        Debug.Log("Received new messages");
        sparkMessageItems = JsonUtility.FromJson<SparkMessageItems>(json);
        UpdateMessages(sparkMessageItems);
    }


    //Goes through the messages received from Spark and updates each avatar. Maintains a record of the last message ID to only add the new ones. 
    private void UpdateMessages(SparkMessageItems messageItems)
    {
        int endIndex = messageItems.items.Length;

        if (lastMessageID != "")
        {
            //We've received messages previously. Find index of lastMessageID; 
            for (int i = 0; i < messageItems.items.Length; i++)
            {
                if (messageItems.items[i].id != lastMessageID) continue;

                endIndex = i;
            }
        }

        for (int i = endIndex - 1; i >= 0; i--)
        {
            if (!HasAvatar(messageItems.items[i].personId))
            {
                //this person is new. Create an Avatar for them.
                CreateAvatar(messageItems.items[i].personId, messageItems.items[i].personEmail);
                //now we can update their messages.
                UpdateAvatar(messageItems.items[i].personId, messageItems.items[i].personEmail);
            }

            UpdateAvatarMessage(messageItems.items[i].personId, messageItems.items[i].text);

        }

        lastMessageID = messageItems.items[0].id;
    }

    private void UpdateAvatarMessage(string personId, string message)
    {
        GetAvatar(personId).UpdateMessages(message);
    }

    #endregion

    #region Avatar Creation

    //Requests the a list of people already in this room
    private void GetRoomAvatars()
    {
        Debug.Log("Getting room avatars..");
        SparkManager.instance.GetMembers("", ParseAvatars);
        updateAll = true;
    }

    // Receives the json stream, and creates new Avatar objects for each personId (if they don't already have one)
    public void ParseAvatars(string json)
    {
        MembershipItems memberships = JsonUtility.FromJson<MembershipItems>(json);


        if (memberships == null || memberships.items == null || memberships.items.Length == 0 )
        {
            Debug.LogWarning("Received empty memberships list for Avatars.");
            return;
        }
        else
        { 
            Debug.Log("Parsing room avatars..");
        }


        StartCoroutine(CreateAvatars(memberships));
    }

    //Cycle to create the avatars in the room, spacing it out over frames -- not sure how many could be in a room
    private IEnumerator CreateAvatars(MembershipItems memberships)
    {
        for (int i = 0; i < memberships.items.Length; i++)
        {
            if (HasAvatar(memberships.items[i].personId)) continue;
            Debug.Log("Creating avatar for " + memberships.items[i].personId);

            CreateAvatar(memberships.items[i].personId, memberships.items[i].personEmail);

            yield return null; //just in case. We're instantiating new Avatars atm.. Remember to pool this. 
        }

        //StartCoroutine(UpdateAvatars());
        if(updateAll) NextAvatar(); //this kicks off the update chain.
    }

    //Spawns the actual avatar object.
    private Avatar CreateAvatar(string personId, string personEmail)
    {
        GameObject newAvatarObject = (GameObject)Instantiate(avatarPrefab);
        newAvatarObject.transform.position = SpawnPoint();
        Avatar newAvatar = newAvatarObject.GetComponent<Avatar>();

        newAvatar.SetPersonId(personId);
        newAvatar.SetEmail(personEmail);
        avatars.Add(newAvatar);

        return newAvatar;
    }

    //picks a spawn point that hasn't been used in this room yet. If we run out, it just spawns them at the first point as a fallback.
    private Vector3 SpawnPoint()
    {
        for(int i = 0; i < avatarSpawnPoints.Length; i++)
        {
            if (avatarSpawnPoints[i].isOccupied) continue;

            avatarSpawnPoints[i].isOccupied = true;
            return avatarSpawnPoints[i].Position();
        }
        return avatarSpawnPoints[0].Position();
    }

    //this is called when all Avatars are being updated to move to the next avatar, once its complete, starts the message cycle
    // this is part of the innitial room setup.
    private void NextAvatar()
    {
        if(currentAvatar >= avatars.Count)
        {
            // Avatars are loaded, start messages routine.
            Debug.Log("Starting Message Cycle");
            StartCoroutine(UpdateMessageCycle());
            updateAll = false;
            return;
        }

        UpdateAvatar(avatars[currentAvatar].personId, avatars[currentAvatar].email);
        currentAvatar++;
    }

    #endregion

    #region Update Avatar's Name and Image from Spark

    //This will trigger a request for user details from Spark 
    private void UpdateAvatar(string personId, string email)
    {
        if (!HasAvatar(personId)) return;

        Debug.Log("Getting update for " + personId);
        SparkManager.instance.GetPerson(personId, email, ParsePerson);
    }

    //Callback for the GET request.  Right now it automatically triggers an Update when it receives the data. 
    public void ParsePerson(string json)
    {
        PersonItems personItems = JsonUtility.FromJson<PersonItems>(json);

        if (personItems == null || personItems.items == null || personItems.items.Length <= 0)
        {
            Debug.LogWarning("Avatar Manager tried to parse a person with no attributes. An Avatar update has failed.");
            return;
        }

        Debug.Log("Parsing update for " + personItems.items[0].id);
        StartCoroutine(UpdateAvatarLoading(personItems));
    }

    //This actually does the update on the Avatar object. It sets the display name and downloads the Avatar's image
    public IEnumerator UpdateAvatarLoading(PersonItems personItems)
    {
        Avatar targetAvatar = GetAvatar(personItems.items[0].id);

        if (targetAvatar == null)
        {
            Debug.LogWarning("No avatar found for person " + personItems.items[0].id + ". Avatar update aborted.");

            if (updateAll) NextAvatar();

            yield break;
        }

        targetAvatar.UpdateName(personItems.items[0].displayName);

        Debug.Log("Downloading avatar picture for " + personItems.items[0].id);
        WWW www = new WWW(personItems.items[0].avatar); //downloading the avatar picture
        yield return www;
    
        Debug.Log("Download complete. Updating Avatar Image for " + personItems.items[0].id);
        targetAvatar.UpdateImage(www.texture);

        if (updateAll) NextAvatar();
    }

    #endregion

    #region Utils

    //Finds the avatar of the personId.
    private Avatar GetAvatar(string personId)
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            if (avatars[i].personId != personId) continue;

            return avatars[i];
        }

        return null; // CreateAvatar(personId); 
    }

    private bool HasAvatar(string personId)
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            if (avatars[i].personId != personId) continue;

            return true;
        }

        return false;
    }

    #endregion
}
