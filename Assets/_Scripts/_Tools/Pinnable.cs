using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Valve.VR.InteractionSystem;

/// <summary>
/// Simple SteamVR Snap position placement, derived from Steam's own Throwable script
/// </summary>
[RequireComponent(typeof(Interactable))]
public class Pinnable : MonoBehaviour
{
    [EnumFlags]
    [Tooltip("The flags used to attach this object to the hand.")]
    public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachFromOtherHand;

    [Tooltip("Name of the attachment transform under in the hand's hierarchy which the object should should snap to.")]
    public string attachmentPoint;

    [Tooltip("When detaching the object, should it return to its original parent?")]
    public bool restoreOriginalParent = false;

    private bool attached = false;

    public UnityEvent onPickUp;
    public UnityEvent onDetachFromHand;


    private void OnHandHoverBegin(Hand hand)
    {
        bool showHint = false;

        if (!attached)
        {
            if (hand.GetStandardInteractionButton())
            {
                    hand.AttachObject(gameObject, attachmentFlags, attachmentPoint);
                    showHint = false;           
            }
        }

        if (showHint)
        {
            ControllerButtonHints.ShowButtonHint(hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
        }
    }


    private void OnHandHoverEnd(Hand hand)
    {
        ControllerButtonHints.HideButtonHint(hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
    }
    

    private void HandHoverUpdate(Hand hand)
    {
        //Trigger got pressed
        if (hand.GetStandardInteractionButtonDown())
        {
            hand.AttachObject(gameObject, attachmentFlags, attachmentPoint);
            ControllerButtonHints.HideButtonHint(hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
        }
    }


    private void OnAttachedToHand(Hand hand)
    {
        attached = true;

        onPickUp.Invoke();

        hand.HoverLock(null);
    }


    private void OnDetachedFromHand(Hand hand)
    {
        attached = false;

        onDetachFromHand.Invoke();

        hand.HoverUnlock(null);

        //transform.position = hand.transform.position;  
    }


    protected virtual void HandAttachedUpdate(Hand hand)
    {
        //Trigger got released
        if (!hand.GetStandardInteractionButton())
        {
            // Detach ourselves late in the frame.
            // This is so that any vehicles the player is attached to
            // have a chance to finish updating themselves.
            // If we detach now, our position could be behind what it
            // will be at the end of the frame, and the object may appear
            // to teleport behind the hand when the player releases it.
            StartCoroutine(LateDetach(hand));
        }
    }


    private IEnumerator LateDetach(Hand hand)
    {
        yield return new WaitForEndOfFrame();

        hand.DetachObject(gameObject, restoreOriginalParent);
    }


    private void OnHandFocusAcquired(Hand hand)
    {
        gameObject.SetActive(true);
    }


    private void OnHandFocusLost(Hand hand)
    {
        gameObject.SetActive(false);
    }
}

