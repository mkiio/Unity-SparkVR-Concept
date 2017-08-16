using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorController : MonoBehaviour {

    private DoorManager doorManager;

    public Room room;
    public GameObject endCapLeft;
    public GameObject endCapRight;
    public Text textRoomName;

    private Transform doorOrigin;
    private Transform roomOrigin;
    private bool doorMoving = false;

    private void Start()
    {
        doorManager = FindObjectOfType<DoorManager>();
        doorOrigin = transform.Find("DoorOrigin");
        roomOrigin = transform.Find("RoomOrigin");
    }

    public void SetRoom(Room _room)
    {
        room = _room;
        textRoomName.text = _room.title;
    }

    public void SetEnd(bool isEnd)
    {
        endCapRight.SetActive(isEnd);
    }

    public void SetBegining(bool isBegining)
    {
        endCapLeft.SetActive(isBegining);
    }

    public void OpenDoor()
    {
        if (doorMoving) return;

        doorManager.CloseAllOtherDoors(room.id);
        doorManager.MoveRoom(roomOrigin.position);
        SparkManager.instance.ChangeRoom(room.id);

        StartCoroutine(MoveDoor(new Vector3(-90f, 0f, -90f)));
    }

    IEnumerator MoveDoor(Vector3 target, float duration = 1.0f)
    {
        doorMoving = true;

        Quaternion startRotation = doorOrigin.rotation;
        Quaternion targetRotation = Quaternion.Euler(target);
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float currentTime = (Time.time - startTime) / duration;
            doorOrigin.rotation = Quaternion.Lerp(startRotation, targetRotation, currentTime);
            yield return null;
        }

        doorOrigin.rotation = targetRotation;

        doorMoving = false;
    }

    public void CloseDoor()
    {
        if (doorMoving) return;

        StartCoroutine(MoveDoor(new Vector3(-90f, 0f, 0f)));
    }

}
