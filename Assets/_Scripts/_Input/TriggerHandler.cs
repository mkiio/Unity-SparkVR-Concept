using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerHandler : MonoBehaviour {

    public UnityEvent OnTrigger = new UnityEvent();
    public string enteredObjectMessage;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered");

        if (OnTrigger != null) OnTrigger.Invoke();

        other.SendMessage(enteredObjectMessage, SendMessageOptions.DontRequireReceiver);
    }
}
