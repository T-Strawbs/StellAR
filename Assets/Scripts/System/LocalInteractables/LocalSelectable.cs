using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LocalSelectable : Selectable
{
    protected override void onSelection(SelectEnterEventArgs args)
    {
        //tell the selection manager to set the current selection to this interactable
        SelectionManager.Instance.setCurrentSelection(interactable);
    }

    protected override void onDeselection(SelectExitEventArgs args)
    {
        //kinda bad pattern use because this method is useless on a local selectable
        //at least for now
    }

}
