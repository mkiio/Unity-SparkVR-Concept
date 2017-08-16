using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour {
    public GameObject doorPrefab;
    public float doorSpacing = 4.0f;
    public Vector3 hallOrigin;

    Transform theRoom;

    private List<DoorController> doorContollers = new List<DoorController>();

    // Use this for initialization
    void Start ()
    {
        theRoom = GameObject.Find("TheRoom").transform;

        if (SparkManager.instance.IsAuthenticated())
        {
            UpdateRooms();
        }
        else
        {
            SparkManager.instance.OnAuthenticated.AddListener(UpdateRooms);
        }
    }
	
    void UpdateRooms()
    {
        if(doorContollers.Count > 0)
        {
            ClearDoors();
        }

        SparkManager.instance.GetRooms(ParseRooms);
    }

    private void ClearDoors()
    {
        foreach(DoorController doorController in doorContollers)
        {
            Destroy(doorController.gameObject);
        }
    }

    public void MoveRoom(Vector3 target)
    {
        theRoom.position = target;
    }

    public void CloseAllOtherDoors(string excludeDoor)
    {
        for(int i = 0; i < doorContollers.Count; i++)
        {
            if(doorContollers[i].room.id != excludeDoor)
            {
                doorContollers[i].CloseDoor();
            }
        }
    }

    private void ParseRooms(string json)
    {
        RoomItems roomItems = JsonUtility.FromJson<RoomItems>(json);
        LayoutDoors(roomItems.items);
    }

    public void LayoutDoors(Room[] rooms)
    {
        for(int i = 0; i < rooms.Length; i++)
        {
            Vector3 newSpawnpoint = hallOrigin + new Vector3(doorSpacing * i, 0, 0);
            GameObject newDoorObject = (GameObject)Instantiate(doorPrefab, newSpawnpoint, Quaternion.identity);
            DoorController newDoorController = newDoorObject.GetComponent<DoorController>();
            newDoorController.SetRoom(rooms[i]);

            if (i == rooms.Length - 1 )
            {
                newDoorController.SetEnd(true);
            }

            doorContollers.Add(newDoorController);
        }
    }
}
