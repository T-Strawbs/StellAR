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

        //set selection to this explodable
        SelectionManager.Instance.setSelection(explodable);
    }
}
