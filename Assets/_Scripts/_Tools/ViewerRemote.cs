using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Tool))]
public class ViewerRemote : MonoBehaviour {

    private RemoteViewerController remoteViewerController;

    public Transform buttonTransform;
    private Vector3 startPosition;
    private Vector3 targetPosition = Vector3.zero;

	void Start ()
    {
        remoteViewerController = FindObjectOfType<RemoteViewerController>();
        startPosition = buttonTransform.localPosition;
        GetComponent<Tool>().OnToolButtonDown.AddListener(TakePicture);
    }
	
    public void TakePicture()
    {
        remoteViewerController.TakePicture();
        StartCoroutine(ButtonPressCycle());
    }

    IEnumerator ButtonPressCycle()
    { 
        buttonTransform.localPosition = targetPosition;
        yield return new WaitForSeconds(2.0f); //this is the hardcoded cooldown of the photo feature -- remember you did this! 
        buttonTransform.localPosition = startPosition;
    }


}
