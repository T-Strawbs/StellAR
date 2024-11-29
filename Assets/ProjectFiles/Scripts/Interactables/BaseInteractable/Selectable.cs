using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Abstract class for facilitating the selectability of interactables so that the system
/// can track the current selection and process it.
/// </summary>
public abstract class Selectable : MonoBehaviour
{
    /// <summary>
    /// reference to the interactable that this selectable belongs to.
    /// </summary>
    public Interactable interactable { get; set; }
    /// <summary>
    /// the object manipulator of this selectable 
    /// that enables XR Interactors to interact
    /// and select this object.
    /// </summary>
    protected ExtendableObjectManipulator manipulator;
    /// <summary>
    /// Property for accesing this selectable's object manipulator
    /// </summary>
    public virtual ExtendableObjectManipulator Manipulator
    {
        get { return manipulator; }
        set
        {
            manipulator = value;
            manipulator.selectEntered.AddListener(onSelection);
            manipulator.selectExited.AddListener(onDeselection);
        }
    }
    /// <summary>
    /// The colldier that the object manipulator will need to be interacted with
    /// </summary>
    public MeshCollider meshCollider;
    /// <summary>
    /// listener method that invokes behaviour when the object manipulator triggers
    /// it's selectEntered event.
    /// </summary>
    /// <param name="args"></param>
    protected abstract void onSelection(SelectEnterEventArgs args);
    /// <summary>
    /// listener method that invokes behaviour when the object manipulator triggers
    /// it's selectExited event.
    /// </summary>
    /// <param name="args"></param>
    protected abstract void onDeselection(SelectExitEventArgs args);
    /// <summary>
    /// method for enabling/disabling the manipulation of this selectable.
    /// </summary>
    /// <param name="isAllowed"></param>
    public virtual void toggleAllowedManipulations(bool isAllowed)
    {
        if (isAllowed)
        {
            //allow all manipulations
            manipulator.AllowedManipulations =
                    MixedReality.Toolkit.TransformFlags.Move |
                    MixedReality.Toolkit.TransformFlags.Rotate |
                    MixedReality.Toolkit.TransformFlags.Scale;
        }
        else
        {
            //revoke all manipulations
            manipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.None;
        }
    }
}