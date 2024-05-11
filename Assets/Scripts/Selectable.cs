using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


/// <summary>
/// *** deprecated ***
/// This is the component we use when we want the object to be selectable.
/// </summary>
[Obsolete("This was the component we'd use when we want the object to be selectable. Moved Functionality to Explodable")]
public class Selectable : MonoBehaviour
{
    [SerializeField] private Collider collider;

    [SerializeField] private SelectableManipulator selectableManipulator;

    protected void Start()
    {
        initialiseSelectable();        
    }

    private void initialiseSelectable()
    {
        //check if were are an empty object (non mesh object)
        if (!GetComponent<MeshRenderer>())
            return;
        //add mesh collider
        collider = transform.AddComponent<MeshCollider>();
        //add selectable manipulator
        selectableManipulator = transform.AddComponent<SelectableManipulator>();

    }



}
