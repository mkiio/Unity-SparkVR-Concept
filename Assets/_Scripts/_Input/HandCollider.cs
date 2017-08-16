using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;


//simple script that contextually allows people to push things around.
public class HandCollider : MonoBehaviour {

    public bool pressButtonToCollide = false;
    private Hand hand;
    private GameObject sphereCollider;
    private Rigidbody _rigodbody;

    void Start ()
    {
        hand = GetComponent<Hand>();
        sphereCollider = transform.Find("HandCollider").gameObject;
        sphereCollider.SetActive(false);
        _rigodbody = GetComponent<Rigidbody>();
    }

    public Rigidbody _Rigidbody()
    {
        return _rigodbody;
    }
	
	void Update ()
    {
		if(pressButtonToCollide)
        {
            ActiveCollider();
        }
        else
        {
            PassiveCollider();
        }
	}

    private void ActiveCollider ()
    {
        if (hand.GetStandardInteractionButton() && !hand.hoveringInteractable && hand.AttachedObjects.Count <= 1)
        {
            if (!sphereCollider.activeSelf)
            {
                sphereCollider.SetActive(true);
            }
        }
        else
        {
            if (sphereCollider.activeSelf)
            {
                sphereCollider.SetActive(false);
            }
        }
    }

    private void PassiveCollider()
    {
        if (hand.GetStandardInteractionButton() || hand.hoveringInteractable || hand.AttachedObjects.Count > 1)
        {
            if (sphereCollider.activeSelf)
            {
                sphereCollider.SetActive(false);
            }
        }
        else
        {
            if (!sphereCollider.activeSelf)
            {
                sphereCollider.SetActive(true);
            }
        }
    }

    public bool IsColliding()
    {
        return sphereCollider.activeSelf;
    }
}
