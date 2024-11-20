using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Abstract Class for interactables. Interactables are objects that can selected and manipulated 
/// by XR Interactors. They can also be exploded so that their child objects can be interacted with
/// individually and can be collapsed to bring the object and its children back together.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    /// <summary>
    /// This interactable's true parent that was provided by the model this 
    /// interactable is a part of. This is different from Transform.Parent as 
    /// a transform's parent can change at runtime wheres Interactable.Parent is
    /// essentially static when initialised.
    /// </summary>
    public Interactable parent { get; set; }
    /// <summary>
    /// The true children of this interactable which were provided by the model
    /// structure this interactable is a part of. This is different from Transform.Children
    /// as that can be changed at runtime whereas Interactable.Children is essentially static
    /// when initialised.
    /// </summary>
    public List<Interactable> children = new List<Interactable>();
    /// <summary>
    /// Property for the selectable that is part of this interactable.
    /// </summary>
    public Selectable selectable { get; set; }
    /// <summary>
    /// Property for the explodable that is a part of this interactable
    /// </summary>
    public Explodable explodable { get; set; }
    /// <summary>
    /// The mesh type of this interactable
    /// </summary>
    public MeshType meshType;
    
    /// <summary>
    /// abstract method for exploding this interactable
    /// </summary>
    public abstract void explodeInteractable();
    /// <summary>
    /// abstract method for collapsing this interactable using the bool
    /// to collapse single or all levels.
    /// </summary>
    /// <param name="isSingleCollapse"></param>
    public abstract void collapseInteractable(bool isSingleCollapse);
    /// <summary>
    /// getter method for this interactable's explosion status
    /// </summary>
    /// <returns></returns>
    public virtual ExplosionStatus explosionStatus()
    {
        return explodable.explosionStatus;
    }

    /// <summary>
    /// Search method for finding a child interactable within the model structure
    /// </summary>
    /// <param name="targetName"></param>
    /// <returns></returns>
    public virtual Interactable findChild(string targetName)
    {
        return findChild(this, targetName);
    }
    /// <summary>
    /// recursive search method for finding a child interactable within the model structure
    /// </summary>
    /// <param name="current"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    protected virtual Interactable findChild(Interactable current, string targetName)
    {
        //search immediate children for the target
        foreach (Interactable child in current.children)
        {
            //check if the child matches the target
            if (child.gameObject.name == targetName)
                return child;
        }
        //the target is not in our immediate children so run a recursive loop
        foreach (Interactable child in current.children)
        {
            //capture the result of the recursive search
            Interactable searchResult = findChild(child, targetName);
            //check if the child matches the target
            if (searchResult)
                return searchResult;
        }
        //we didnt find the target
        return null;
    }

    /// <summary>
    /// Finds a child object by name. Only searches direct children.
    /// </summary>
    /// <param name="targetName"></param>
    /// <returns>Target GameObject if exists, else null if not found.</returns>
    public GameObject findNamedChildDirect(string targetName)
    {
        foreach (Interactable child in this.children)
        {
            if (child.gameObject.name == targetName)
            {
                return child.gameObject;
            }
        }
        return null;
    }

}

/// <summary>
/// Enum for labeling if an interactable has a mesh or not.
/// Makes it quicker and easier to check instead of searching for a 
/// mesh renderer of a type.
/// </summary>
public enum MeshType { NON_MESH, MESH }