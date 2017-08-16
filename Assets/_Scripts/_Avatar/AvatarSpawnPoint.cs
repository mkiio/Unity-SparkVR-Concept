using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSpawnPoint : MonoBehaviour {

    public bool isOccupied;

    public Vector3 Position()
    {
        return transform.position;
    }
}
