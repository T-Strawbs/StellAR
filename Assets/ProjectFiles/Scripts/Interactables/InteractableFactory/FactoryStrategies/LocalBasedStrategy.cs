
using Unity.VisualScripting;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     � Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     � Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// FactoryStrategy concretion for initialising Gameobjects as LocalBasedInteractable
/// </summary>
public class LocalBasedStrategy : FactoryStrategy
{
    public void initialiseInteractable(GameObject interactableObject)
    {
        initialiseInteractables(interactableObject.transform, null);
        //DebugConsole.Instance.LogDebug($"Initialising __ {interactableObject.name} __");
    }

    /// <summary>
    /// Recursive method for initialising each object within the given model's object tree to
    /// be a localbased interactable.
    /// </summary>
    /// <param name="current">the current object of the model tree</param>
    /// <param name="parent">the parent object of the current object</param>
    private void initialiseInteractables(Transform current, Transform parent)
    {
        //add an interactable component to the current object
        LocalBasedInteractable currentInteractable = current.AddComponent<LocalBasedInteractable>();
        //add an explodable component to the current object
        LocalExplodable currentExplodable = current.AddComponent<LocalExplodable>();
        //set the currentInteractables explodable
        currentInteractable.explodable = currentExplodable;
        //give the explodable a reference to the current interactable
        currentExplodable.interactable = currentInteractable;

        //check if the parent exists
        if (!parent)
        {
            //it doesnt exist so set the current interactables parent to null marking it as  root
            currentInteractable.parent = null;
        }
        else
        {
            //the parent exists so grab a ref of the parent's interactable component
            LocalBasedInteractable parentInteractable = parent.GetComponent<LocalBasedInteractable>();
            //set the current interactables parent
            currentInteractable.parent = parentInteractable;
            //add the current interactable as a child of parent
            parentInteractable.children.Add(currentInteractable);
        }

        //we have to assign the MessageBasedSelectable components using depth first recursion due to some weird setup
        //MTRK3s object manipulator (OM) has but my guess is that its to do with how the OM calculates what
        //constitutes the object its attached to by exploring its descendents via recursion.

        //Anyway, for each child of the current game object
        for (int i = 0; i < current.transform.childCount; i++)
        {
            //recursively intitialise the descendants of the the current interactable, depth first
            initialiseInteractables(current.transform.GetChild(i), current);
        }

        //add a selectable component to the current game object
        LocalSelectable currentSelectable = current.AddComponent<LocalSelectable>();
        //set the current interactables selectable
        currentInteractable.selectable = currentSelectable;
        //give the selectable a reference to the current interactable
        currentSelectable.interactable = currentInteractable;

        //check if current object has a mesh of somekind
        if (!current.GetComponent<MeshRenderer>() && !current.GetComponent<SkinnedMeshRenderer>())
        {
            //set the MeshType of this interactable to non Mesh
            currentInteractable.meshType = MeshType.NON_MESH;
        }
        else
        {
            //set the MeshType of this interactable to Mesh
            currentInteractable.meshType = MeshType.MESH;
            //give the current interactable a mesh collider and assign the selectables instance var
            currentSelectable.meshCollider = current.AddComponent<MeshCollider>();           
        }
        //add an object manipulator to the the selectable -- its important for mesh interactables that we do this
        //after adding a mesh collider otherwise the mesh interactables object manipulator wont calculate its bounds
        currentSelectable.Manipulator = current.AddComponent<ExtendableObjectManipulator>();
        //check if the current interactable is a leaf
        if (current.childCount < 1)
        {
            //set the explodable status to leaf
            currentExplodable.explosionStatus = ExplosionStatus.LEAF;
            currentSelectable.Manipulator.enabled = false;
        }
        //its not so check if its the root
        else if (!currentInteractable.parent)
        {
            //set the explodable status to explodable
            currentExplodable.explosionStatus = ExplosionStatus.EXPLODABLE;
        }
        else
        {
            //its not so we want to make the current explodable status to inactive
            currentExplodable.explosionStatus = ExplosionStatus.INACTIVE;
            //and we want to turn off the selectables object manipulator
            currentSelectable.Manipulator.enabled = false;
        }
    }
}