using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eraser : MonoBehaviour {

    private LineManager lineManager;
    public Transform point;
    

    void Start()
    {
        GetComponent<Tool>().OnToolButtonDown.AddListener(EraseAtPoint);
        lineManager = FindObjectOfType<LineManager>();
    }

    private void EraseAtPoint()
    {
        lineManager.EraseLine(point.position);
    }
}
