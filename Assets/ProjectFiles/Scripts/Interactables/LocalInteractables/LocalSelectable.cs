using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
///

/// <summary>
/// concrete class for Selectables that are handled offline.
/// </summary>
public class LocalSelectable : Selectable
{
    protected override void onSelection(SelectEnterEventArgs args)
    {
        //tell the selection manager to set the current selection to this interactable
        SelectionManager.Instance.setCurrentSelection(interactable);
    }

    protected override void onDeselection(SelectExitEventArgs args)
    {
        //this is kind of a bad implementation of an abstract class as this method
        //currently has no need to be deselected when its a local only selection
    }

}
