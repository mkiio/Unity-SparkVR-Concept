using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Tool))]
public class Marker : MonoBehaviour
{
    public Color color;

    public Material lineMaterial;
    private bool drawing = false;
    public Transform capsule;
    public Transform tip;
    public float pointDifference = 0.1f;
    private Vector3 capsuleClosedPosition;
    public Vector3 capsuleOpenPosition;

    private List<Vector3> points = new List<Vector3>();
    private LineRenderer currentRenderer;
    private LineManager lineManager;


    void Start()
    {
        GetComponent<Tool>().OnToolButtonDown.AddListener(StartDraw);
        GetComponent<Tool>().OnToolButtonUp.AddListener(EndDraw);
        GetComponent<Pinnable>().onDetachFromHand.AddListener(EndDraw);
        lineManager = FindObjectOfType<LineManager>();

        capsuleClosedPosition = capsule.localPosition;
    }

    void Update()
    {
        if (!drawing) return;

        float distanceFromLastPoint = (tip.position - points[points.Count-1]).sqrMagnitude;

        if (pointDifference * pointDifference > distanceFromLastPoint)
        {
            points.Add(tip.position);
            UpdateLine();
        }
    }

    private void StartDraw()
    {
        if (drawing) return;

        drawing = true;
        capsule.localPosition = capsuleOpenPosition;
        points.Add(tip.position);
        Debug.Log("Starting line at " + tip.position);
        currentRenderer = lineManager.CreateLine(lineMaterial);
    }

    private void UpdateLine ()
    {
        currentRenderer.positionCount = points.Count;
        Vector3[] positions = points.ToArray();
        currentRenderer.SetPositions(positions);
    }

    private void EndDraw()
    {
        if (!drawing) return;

        drawing = false;
        currentRenderer = null;
        points.Clear();

        capsule.localPosition = capsuleClosedPosition;
    }
}
