using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public Interactable parent { get; set; }

    public List<Interactable> children = new List<Interactable>();

    public virtual Interactable findChild(string targetName)
    {
        return findChild(this, targetName);
    }
    public Selectable selectable { get; set; }
    public Explodable explodable { get; set; }

    public MeshType meshType;

    public abstract void explodeInteractable();
    public abstract void collapseInteractable(bool isSingleCollapse);

    public virtual ExplosionStatus explosionStatus()
    {
        return explodable.explosionStatus;
    }

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

public enum MeshType { NON_MESH, MESH }