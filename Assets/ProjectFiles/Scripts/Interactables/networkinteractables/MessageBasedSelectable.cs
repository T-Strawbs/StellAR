using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
///

/// <summary>
/// Concretion of the Selectable class for Interactbles that are handled over the network
/// using custom messaging systems.
/// </summary>
public class MessageBasedSelectable : Selectable
{
    /// <summary>
    /// Executes behaviour when this object is selected.
    /// </summary>
    /// <param name="args">
    /// We dont need to do anything with this param, its just there 
    /// because Manipulator.SelectEntered requires it.
    /// </param>
    protected override void onSelection(SelectEnterEventArgs args)
    {
        //request the MessageBasedInstanceManager to give us ownership of this object
        if (interactable is MessageBasedInteractable messageBasedInteractable)
            messageBasedInteractable.requestOwnership();
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
    protected override void onDeselection(SelectExitEventArgs args)
    {
        //tell the MessageBasedInstanceManager we dont need ownership anymore
        if(interactable is MessageBasedInteractable messageBasedInteractable)
            messageBasedInteractable.revokeOwnership();
    }
}
