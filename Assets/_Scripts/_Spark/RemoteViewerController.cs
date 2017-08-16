using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RemoteViewerController : MonoBehaviour
{
    private RenderTexture viewerRenderTexture;
    private Camera viewerCamera;

    private float coolDown = 5.0f;
    private float lastPicture;

    void Start()
    {
        viewerCamera = GetComponentInChildren<Camera>();

        if (viewerCamera == null)
        {
            Debug.LogError("No camera found for remote viewer. Remove viewer was not setup.");
            return;
        }

        viewerRenderTexture = viewerCamera.targetTexture;
    }

    public void TakePicture()
    {
        if (!IsReady())
        {
            return;
        }

        StartCoroutine(TakePictureCycle());
        lastPicture = Time.time;
    }

    //Grabs the content of the current RenderTexture from this camera, enclodes it to PNG and transfers it via SparkManager
    private IEnumerator TakePictureCycle()
    {
        yield return new WaitForEndOfFrame(); //Unity Docs -- Only read the screen buffer after rendering is complete. I trust 'em. 

        RenderTexture.active = viewerRenderTexture;

        Texture2D newPicture = new Texture2D(viewerRenderTexture.width, viewerRenderTexture.height, TextureFormat.RGB24, false);
        newPicture.ReadPixels(new Rect(0, 0, viewerRenderTexture.width, viewerRenderTexture.height), 0, 0);
        newPicture.Apply();

        byte[] bytes = newPicture.EncodeToPNG();

        //To write the file locally, was just using this for testing. 
        //string filePath = Application.dataPath + "/Resources/Output/Picture.png";
        //File.WriteAllBytes(filePath, bytes);

        SparkManager.instance.SendLocalFile(bytes);
    }

    private bool IsReady()
    {
        if (viewerCamera == null || viewerRenderTexture == null)
        {
            Debug.LogError("Remote viewer is not ready, no camera or render texture specified");
            return false;
        }

        if(Time.time - lastPicture < coolDown)
        {
            return false;
        }

        return true;
    }
}
