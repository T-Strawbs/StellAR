using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Selectable : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public Interactable interactable { get; set; }

    /// <summary>
    /// The component that allows the user to select and manipulate the transform of this object
    /// via the mrtk3 toolkit.
    /// </summary>
    public MessageBasedManipulator manipulator;// { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public MessageBasedManipulator Manipulator
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
    public MeshCollider meshCollider;// { get; set; }

    /// <summary>
    /// Executes behaviour when this object is selected.
    /// </summary>
    /// <param name="args">
    /// We dont need to do anything with this param, its just there 
    /// because Manipulator.SelectEntered requires it.
    /// </param>
    private void onSelection(SelectEnterEventArgs args)
    {
        //request the NetworkInteractableInstanceManager to give us ownership of this object
        interactable.requestOwnership();
        //update the local Selection Manager to set this as the current selectable
        SelectionManager.Instance.setCurrentSelection(interactable);
    }

    /// <summary>
    /// Executes behaviour when this object is deselected.
    /// </summary>
    /// <param name="args">
    /// We dont need to do anything with this param, its just there 
    /// because Manipulator.SelectExited requires it.
    /// </param>
    private void onDeselection(SelectExitEventArgs args)
    {
        //tell the NetworkInteractableInstanceManager we dont need ownership anymore
        interactable.revokeOwnership();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isAllowed"></param>
    public void toggleAllowedManipulations(bool isAllowed)
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
