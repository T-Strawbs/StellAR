using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosiveInitialiser : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //initialise explosive GOs
        initialise(transform);
    }
    
    /// <summary>
    /// Recursively initialises each child GO starting from the parent
    /// </summary>
    private void initialise(Transform hostTransform)
    {
        initialise(transform,hostTransform);
    }

    /// <summary>
    /// Recursive Method for initialising the GOs
    /// </summary>
    /// <param name="parent"></param>
    private void initialise(Transform parent,Transform hostTransform)
    {
        //add explodable component
        Explodable parentExplodable = parent.AddComponent<Explodable>();
        //if the parent has a mesh renderer
        MeshRenderer parentRenderer = parent.GetComponent<MeshRenderer>();
        Debug.Log($" parent renderer of {transform.name} is {parentRenderer == null}");
        if (parentRenderer)
        {
            //add mesh collider
            parent.transform.AddComponent<MeshCollider>();
            //add selectable manipulator
            SelectableManipulator selectableManipulator = parent.transform.AddComponent<SelectableManipulator>();
            //set the selectableManipulator's explodable to the parent explodable
            selectableManipulator.setExplodable(parentExplodable);
            //set the selectableManipulators hostTransformt he the root transform
            selectableManipulator.HostTransform = hostTransform;
            //set the explodables selectableManipulator
            parentExplodable.setSelectableManipulator(selectableManipulator);
        }
        //for each child
        for (int i = 0; i < parent.childCount;i++)
        {
            //recursively initialise children
            Transform child = parent.GetChild(i);
            //if null
            if (!child)
                return;
            initialise(child,hostTransform);
        }

    }
}
