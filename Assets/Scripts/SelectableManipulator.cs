using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectableManipulator : ObjectManipulator
{
    private void Start()
    {
        selectMode = InteractableSelectMode.Multiple;

        
    }

    [SerializeField] private Explodable explodable;
    public void setExplodable(Explodable explodable)
    {
        this.explodable = explodable;
    }
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);

        //we want to get the highest in the tree predecessing explodable that is able to explode
        //so that we can set it as our current selection. If the direct parent has exploded then
        //this is a leaf child and we set this as the current selection
        //Explodable explodablePredecessor = explodable.findExplodablePredecessor();

        //set selection to highest explodable
        //SelectionManager.Instance.setSelection(explodablePredecessor);

        //set selection to this explodable
        SelectionManager.Instance.setSelection(explodable);
    }
}
