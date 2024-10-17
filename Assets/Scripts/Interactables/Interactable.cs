using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType { NON_MESH, MESH }
public class Interactable : NetworkInteractable
{
    public Explodable explodable { get; set; }

    public MeshType meshType;

    public Interactable findChild(string targetName)
    {
        return findChild(this, targetName);
    }

    private Interactable findChild(Interactable current,string targetName)
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
            if(searchResult)
                return searchResult;
        }
        //we didnt find the target
        return null;
    }

    public void explodeInteractable()
    {
        explodable.explode();
    }

    public void collapseInteractable(bool isSingleCollapse)
    {
        if(isSingleCollapse)
        {
            explodable.collapseSingle();
        }
        else
        {
            explodable.collapseAll();
        }
    }

    public bool canExplode()
    {
        return explodable.canExplode();
    }

    public bool areCollapsablesOwnedSingle()
    {
        return explodable.areCollapsablesOwnedSingle();
    }

    public bool areCollapsablesOwnedAll()
    {
        return explodable.areCollapsablesOwnedAll();
    }

    public bool areCollapsablesLockedSingle()
    {
        bool t = explodable.areCollapsablesLockedSingle();
        DebugConsole.Instance.LogDebug($"{name} can collapse single as not locked? {t}");
        return explodable.areCollapsablesLockedSingle();
    }

    public bool areCollapsablesLockedAll()
    {
        bool t = explodable.areCollapsablesLockedSingle();
        DebugConsole.Instance.LogDebug($"{name} can collapse all as not locked? {t}");
        return explodable.areCollapsablesLockedALL();
    }

    public List<Interactable> getCollapsableInteractables(bool isSingleCollapse)
    {
        //create the collapsableInteractables list
        List<Interactable> collapsableInteractables = new List<Interactable>();

        //check if this was for a single collapse
        if(isSingleCollapse)
        {
            //check if this interactable is the root
            if(parent == null)
            {
                //add this interactable to the list
                collapsableInteractables.Add(this);
                getCollapsableInteractables(this, ref collapsableInteractables);
            }
            else
            {
                //add the parent to the list
                collapsableInteractables.Add((Interactable)parent);
                getCollapsableInteractables((Interactable)parent, ref collapsableInteractables);
            }
        }
        else
        {
            //find the root interactable
            Interactable rootInteractable = this;
            while(rootInteractable.parent != null)
            {
                rootInteractable = (Interactable)rootInteractable.parent;
            }
            //add the root interactable to the list
            collapsableInteractables.Add(rootInteractable);
            getCollapsableInteractables(rootInteractable, ref collapsableInteractables);

            Debug.Log($"ALL: geting all collapsables");
            foreach (Interactable i in collapsableInteractables)
            {
                Debug.Log($"ALL: {i.name} can be collapsable");
            }
        }
        return collapsableInteractables;
    }
    private void getCollapsableInteractables(Interactable currentInteractable, ref List<Interactable> collapsableInteractables)
    {
        foreach(Interactable child in currentInteractable.children)
        {
            //check if the child is able to explode or is a leaf
            if (child.explosionStatus() == ExplosionStatus.EXPLODABLE ||
                child.explosionStatus() == ExplosionStatus.LEAF)
            {
                collapsableInteractables.Add(child);
            }
            //check if child has exploded
            else if (child.explosionStatus() == ExplosionStatus.EXPLODED)
            {
                getCollapsableInteractables(child, ref collapsableInteractables);
            }
        }
    }

    public ExplosionStatus explosionStatus()
    {
        return explodable.explosionStatus;
    }

    public override void requestOwnership()
    {
        NetworkInteractableInstanceManager.Instance.requestOwnershipOfNetworkInteractable(this);
    }

    public override void revokeOwnership()
    {
        NetworkInteractableInstanceManager.Instance.revokeOwnershipOfNetworkInteractable(this);
    }

    public override void requestUpdateTransform(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        NetworkInteractableInstanceManager.Instance.requestUpdateNetworkInteractableTransform(this, newPosition, newRotation, newScale);
    }

    public override void updateTransformLocalClient(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        transform.position = Vector3.Lerp(transform.position, newPosition, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, newScale, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
    }


    public override void initialiseLookupData
        (
            NetworkInteractable currentInteractble, ref List<NetworkInteractable> listedInteractables, int parentKeyValue
        )
    {
        //add the current interactable to the listed collapsableInteractables list
        listedInteractables.Add(currentInteractble);

        //create a new lookUp Data object
        NetworkInteractableLookupData lookupData = new NetworkInteractableLookupData()
        {
            //set the parent key reference
            parentKey = parentKeyValue,
            //set the object index as the index the current interactable is inserted at
            objectIndex = listedInteractables.IndexOf(currentInteractble)
        };

        currentInteractble.lookupData = lookupData;

        foreach(Interactable child in currentInteractble.GetComponent<Interactable>().children)
        {
            initialiseLookupData(child, ref listedInteractables, parentKeyValue);
        }
    }

    public override void toggleOwnershipLockout(bool isLockRequest)
    {
        if (isLockRequest)
        {
            //set the ownership id of the current interactable to locked
            setOwnerID(GlobalConstants.OWNERSHIP_LOCKED);
        }
        else
        {
            //set the ownership id of the current interactable to unowned
            setOwnerID(GlobalConstants.OWNERSHIP_UNOWNED);
        }
    }

    public override void toggleOwnershipLockoutRecursive(NetworkInteractable currentInteractble, bool isLockRequest)
    {
        //call the toggle lockout method from the currentINteractable
        currentInteractble.toggleOwnershipLockout(isLockRequest);

        //interate over the interactbles children and recursively call this method
        foreach (Interactable child in currentInteractble.GetComponent<Interactable>().children)
        {
            toggleOwnershipLockoutRecursive(child, isLockRequest);
        }
    }

    

   
}