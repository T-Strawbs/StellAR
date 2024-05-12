using Unity.VisualScripting;
using UnityEngine;

public class ExplosiveInitialiser : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        //initialise explosive GOs
        initialiseExplodable(transform);
        //initialise selectable manipulators
        initialiseManipulators(transform,transform);
    }

    /// <summary>
    /// Recursive Method for initialising the GOs
    /// </summary>
    /// <param name="currentObject"></param>
    private void initialiseExplodable(Transform currentObject)
    {
        //add explodable component
        Explodable parentExplodable = currentObject.AddComponent<Explodable>();
        //if the current object has a mesh renderer
        MeshRenderer parentRenderer = currentObject.GetComponent<MeshRenderer>();
        
        Debug.Log($" parent renderer of {transform.name} is {parentRenderer == null}");
        if (parentRenderer)
        {
            //add mesh collider
            currentObject.transform.AddComponent<MeshCollider>();
        }
        //for each child
        for (int i = 0; i < currentObject.childCount;i++)
        {
            //recursively initialise children
            Transform child = currentObject.GetChild(i);
            //if null
            if (!child)
                return;
            initialiseExplodable(child);
        }
    }

    private void initialiseManipulators(Transform currentObject,Transform rootTransform)
    {
        //for each child of the current object
        for(int i = 0; i < currentObject.childCount; i++)
        {
            //get a ref of the child
            Transform child = currentObject.GetChild(i);
            //recursively call initialise manipulators
            initialiseManipulators(child, rootTransform);
        }
        //get the current objects explodable component
        Explodable explodable = currentObject.GetComponent<Explodable>();
        if(!explodable)
            Debug.Log($"Something has gone horribly wrong because {currentObject.name} does not have an explodable");
        //add selectable manipulator
        SelectableManipulator selectableManipulator = currentObject.AddComponent<SelectableManipulator>();
        if (explodable)
        {
            //set the current object's manipulator's explodable ref to the current objects explodable
            selectableManipulator.setExplodable(explodable);
            // do the same in reverse
            explodable.setSelectableManipulator(selectableManipulator);
        }
        //disable the selectable manipulator if the current object is not the root object
        if (currentObject != rootTransform)
            selectableManipulator.enabled = false;
            
    }
}
