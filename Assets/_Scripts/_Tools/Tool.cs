using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

/// <summary>
/// Tool is something that while held, changes your input. Inherits Pinnable Behaviour.
/// </summary>
[RequireComponent(typeof(Interactable))]
public class Tool : Pinnable
{
    public UnityEvent OnToolButtonDown = new UnityEvent();
    public UnityEvent OnToolButtonUp = new UnityEvent();

    protected override void HandAttachedUpdate(Hand hand)
    {
        base.HandAttachedUpdate(hand);

        if (ToolButtonDown(hand))
        {
            if (OnToolButtonDown != null) OnToolButtonDown.Invoke();
        }

        if(ToolButtonUp(hand))
        {
            if (OnToolButtonUp != null) OnToolButtonUp.Invoke();
        }
    }

    private bool ToolButtonDown(Hand hand)
    {
    
        if (hand.noSteamVRFallbackCamera != null)
        {
            return Input.GetKeyDown(KeyCode.T);
        }
        else
        {
            return hand.controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
        }
    }

    private bool ToolButtonUp(Hand hand)
    {

        if (hand.noSteamVRFallbackCamera != null)
        {
            return Input.GetKeyUp(KeyCode.T);
        }
        else
        {
            return hand.controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
        }
    }

}
