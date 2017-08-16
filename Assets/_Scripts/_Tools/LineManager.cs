using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    public float eraseRadius = 1f;
    public List<LineRenderer> lines = new List<LineRenderer>();

    public LineRenderer CreateLine(Material lineMaterial)
    {
        GameObject newLineObject = new GameObject();

        LineRenderer newLine = newLineObject.AddComponent<LineRenderer>();
        newLine.material = lineMaterial;

        newLine.startWidth = 0.01f;
        newLine.endWidth = 0.01f;
        newLine.name = "Line" + lines.Count;

        lines.Add(newLine);   

        return newLine;

    }

    public void EraseLine(Vector3 eraseOrigin)
    {

        if (lines.Count <= 0) return;

        for(int i = 0; i < lines.Count; i++)
        {
            LineRenderer currentLineRender = lines[i];

            Vector3[] linePoints = new Vector3[currentLineRender.positionCount];

            currentLineRender.GetPositions(linePoints);

            for(int j = 0; j < linePoints.Length; j++)
            {
                float distanceFromOrigin = (eraseOrigin - linePoints[j]).sqrMagnitude;

                if(eraseRadius * eraseRadius > distanceFromOrigin)
                {
                    lines.RemoveAt(i);
                    Destroy(currentLineRender);
                    return;
                }
            }

          
        }
    }
}
