using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Concretion of the Interactable class for interacables that are synchronised 
/// over the network via custom messaging.
/// </summary>
public class MessageBasedInteractable : Interactable 
{
    /// <summary>
    /// the id of the client that owns this object
    /// </summary>
    [SerializeField] protected ulong ownerID;

    /// <summary>
    /// The data struct that references this parent's key and this object's index
    /// in order for the MessageBasedInstanceManager to search this object.
    /// </summary>
    public NetworkInteractableLookupData lookupData { get; set; }

    protected void Start()
    {
        //initialise the owner ID as the unowned value
        ownerID = GlobalConstants.OWNERSHIP_UNOWNED;
    }
   
    /// <summary>
    /// checks if the networkinteractable is owned by anyone.
    /// </summary>
    /// <returns>
    /// bool: 
    ///     - True:  someone owns the object
    ///     - False: no one owns the object
    /// </returns>
    public bool isOwned()
    {
        if (ownerID != GlobalConstants.OWNERSHIP_UNOWNED && ownerID != GlobalConstants.OWNERSHIP_LOCKED)
            return true;
        return false;
    }
    /// <summary>
    /// Method to check if the local client owns this object
    /// </summary>
    /// <returns>
    /// bool: 
    ///     - True: the local client owns the object
    ///     - False: the local client does not own the object
    /// </returns>
    public bool isOwnedByLocalClient()
    {
        if (ownerID == NetworkManager.Singleton.LocalClientId)
            return true;
        return false;
    }

    /// <summary>
    /// Checks if this network interacble is locked
    /// </summary>
    /// <returns>
    /// bool: 
    ///     - True: the object is locked
    ///     - False: the object is not locked
    /// </returns>
    public bool isLocked()
    {
        if (ownerID == GlobalConstants.OWNERSHIP_LOCKED)
            return true;
        return false;
    }

    /// <summary>
    /// getter method for this objects owner ID
    /// </summary>
    /// <returns> ulong: The ID of the client that owns this object </returns>
    public ulong getOwnerID()
    {
        return ownerID;
    }

    /// <summary>
    /// Setter method that sets the owner ID of this object
    /// </summary>
    /// <param name="clientID">The ID of the client that now owns the object</param>
    /// <returns> ulong: The ID of the client that owns this object </returns>
    public ulong setOwnerID(ulong clientID)
    {
        return ownerID = clientID;
    }

    /// <summary>
    /// method for invoking the explosion behaviour of this interactable
    /// </summary>
    public override void explodeInteractable()
    {
        explodable.explode();
    }

    /// <summary>
    /// method for invoking the collapsing behaviour of this interactable.
    /// </summary>
    /// <param name="isSingleCollapse">
    /// bool for marking if this interactable should collapse one level or all levels
    /// </param>
    public override void collapseInteractable(bool isSingleCollapse)
    {
        //determine if this interactble should collapse one level or all levels.
        if(isSingleCollapse)
        {
            //cast this interactable's explodable as a MessageBasedExplodable
            if (explodable is MessageBasedExplodable messageBasedExplodable)
                messageBasedExplodable.collapseSingle();
            else
            {
                DebugConsole.Instance.LogError($"MBI_collapseInteractable(): {name} does not have a MB_Explodable");
            }
        }
        else
        {
            //cast this interactable's explodable as a MessageBasedExplodable
            if (explodable is MessageBasedExplodable messageBasedExplodable)
                messageBasedExplodable.collapseAll();
            else
            {
                DebugConsole.Instance.LogError($"MBI_collapseInteractable(): {name} does not have a MB_Explodable");
            }
        }
    }

    /// <summary>
    /// method for checking if this interactable can explode
    /// </summary>
    /// <returns>bool</returns>
    public bool canExplode()
    {
        if(explodable is MessageBasedExplodable messageBasedExplodable)
            return messageBasedExplodable.canExplode();
        else
        {
            DebugConsole.Instance.LogError($"MBI_canExplode(): {name} does not have a MB_Explodable");
            return false;
        }
    }

    /// <summary>
    /// method for checking if the all the interactables involved in collapsing this interactable by one level
    /// are owned by any client
    /// </summary>
    /// <returns>bool</returns>
    public bool areCollapsablesOwnedSingle()
    {
        if (explodable is MessageBasedExplodable messageBasedExplodable)
            return messageBasedExplodable.areCollapsablesOwnedSingle();
        else
        {
            DebugConsole.Instance.LogError($"MBI_areCollapsablesOwnedSingle(): {name} does not have a MB_Explodable");
            return false;
        }
    }

    /// <summary>
    /// method for checking if the all the interactables involved in collapsing this interactable completely
    /// are owned by any client
    /// </summary>
    /// <returns>bool</returns>
    public bool areCollapsablesOwnedAll()
    {
        if (explodable is MessageBasedExplodable messageBasedExplodable)
            return messageBasedExplodable.areCollapsablesOwnedAll();
        else
        {
            DebugConsole.Instance.LogError($"MBI_areCollapsablesOwnedAll(): {name} does not have a MB_Explodable");
            return false;
        }
    }

    /// <summary>
    /// method for checking if the all the interactables involved in collapsing this interactable by one level
    /// are locked.
    /// </summary>
    /// <returns>bool</returns>
    public bool areCollapsablesLockedSingle()
    {
        if (explodable is MessageBasedExplodable messageBasedExplodable)
        {
            bool t = messageBasedExplodable.areCollapsablesLockedSingle();
            DebugConsole.Instance.LogDebug($"{name} can collapse single as not locked? {t}");
            return messageBasedExplodable.areCollapsablesLockedSingle();
        }
        else
        {
            DebugConsole.Instance.LogError($"MBI_areCollapsablesLockedSingle(): {name} does not have a MB_Explodable");
            return false;
        }
        
    }

    /// <summary>
    /// method for checking if the all the interactables involved in collapsing this interactable completely
    /// are locked.
    /// </summary>
    /// <returns>bool</returns>
    public bool areCollapsablesLockedAll()
    {
        if (explodable is MessageBasedExplodable messageBasedExplodable)
        {
            bool t = messageBasedExplodable.areCollapsablesLockedSingle();
            DebugConsole.Instance.LogDebug($"{name} can collapse all as not locked? {t}");
            return messageBasedExplodable.areCollapsablesLockedAll();
        }
        else
        {
            DebugConsole.Instance.LogError($"MBI_canExplode(): {name} does not have a MB_Explodable");
            return false;
        }
        
    }

    /// <summary>
    /// getter method for retrieving a list off all interactables involved in a collaspe process starting
    /// from this interactable. 
    /// </summary>
    /// <param name="isSingleCollapse">marker for if the collaspe would be a single level or a complete collapse</param>
    /// <returns>List<MessageBasedInteractable>: list of interactables that would be involved in a collapse.</returns>
    public List<MessageBasedInteractable> getCollapsableInteractables(bool isSingleCollapse)
    {
        //create the collapsableInteractables list
        List<MessageBasedInteractable> collapsableInteractables = new List<MessageBasedInteractable>();

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
                collapsableInteractables.Add((MessageBasedInteractable)parent);
                getCollapsableInteractables((MessageBasedInteractable)parent, ref collapsableInteractables);
            }
        }
        else
        {
            //find the root interactable
            MessageBasedInteractable rootInteractable = this;
            while(rootInteractable.parent != null)
            {
                rootInteractable = (MessageBasedInteractable)rootInteractable.parent;
            }
            //add the root interactable to the list
            collapsableInteractables.Add(rootInteractable);
            getCollapsableInteractables(rootInteractable, ref collapsableInteractables);

            Debug.Log($"ALL: geting all collapsables");
            foreach (MessageBasedInteractable i in collapsableInteractables)
            {
                Debug.Log($"ALL: {i.name} can be collapsable");
            }
        }
        return collapsableInteractables;
    }
    /// <summary>
    /// Depth first recursive method for buidling a list of all interactables involved in a collapse
    /// </summary>
    /// <param name="currentInteractable"></param>
    /// <param name="collapsableInteractables"></param>
    private void getCollapsableInteractables(MessageBasedInteractable currentInteractable, ref List<MessageBasedInteractable> collapsableInteractables)
    {
        //for each child interactable of the current interactable
        foreach(MessageBasedInteractable child in currentInteractable.children)
        {
            //check if the child is able to explode or is a leaf as we can only collapse interactables 
            //that are loose.
            if (child.explosionStatus() == ExplosionStatus.EXPLODABLE ||
                child.explosionStatus() == ExplosionStatus.LEAF)
            {
                collapsableInteractables.Add(child);
            }
            //check if child has exploded
            else if (child.explosionStatus() == ExplosionStatus.EXPLODED)
            {
                //recursively build the list now continuing from the child.
                getCollapsableInteractables(child, ref collapsableInteractables);
            }
        }
    }

    /// <summary>
    /// method for requesting the instance manager for this client to gain ownership of this interactable
    /// </summary>
    public void requestOwnership()
    {
        MessageBasedInstanceManager.Instance.requestOwnershipOfNetworkInteractable(this);
    }

    /// <summary>
    /// method for requesting the instance manager for this client to revoke ownership of this interactable
    /// </summary>
    public void revokeOwnership()
    {
        MessageBasedInstanceManager.Instance.revokeOwnershipOfNetworkInteractable(this);
    }

    /// <summary>
    /// method for requesting the instance manager to update the transform of this interactable 
    /// over the network.
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="newRotation"></param>
    /// <param name="newScale"></param>
    public void requestUpdateTransform(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        MessageBasedInstanceManager.Instance.requestUpdateNetworkInteractableTransform(this, newPosition, newRotation, newScale);
    }

    /// <summary>
    /// method for locally updating this interactable's global transform's only on the initial client
    /// who made the update transform request. 
    /// Recipient clients use updateTransformLocalClientLocalPosition instead.
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="newRotation"></param>
    /// <param name="newScale"></param>
    public void updateTransformLocalClient(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        transform.position = Vector3.Lerp(transform.position, newPosition, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, newScale, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
    }
    /// <summary>
    /// method for locally updating this interacable's local transforms for clients that didn't make the initial
    /// update transform request. The initial client uses updateTransformLocalClient.
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="newRotation"></param>
    /// <param name="newScale"></param>
    public void updateTransformLocalClientLocalPosition(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
    {
        transform.localPosition = Vector3.Lerp(transform.position, newPosition, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.rotation, newRotation, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, newScale, GlobalConstants.TRANSLATION_SPEED / Time.deltaTime);
    }

    /// <summary>
    /// method for initialising the Lookup data for this interactable so that it can be
    /// found via MessageBasedInstanceManager's lookup table.
    /// </summary>
    /// <param name="currentInteractble"></param>
    /// <param name="listedInteractables"></param>
    /// <param name="parentKeyValue"></param>
    public void initialiseLookupData
        (
            MessageBasedInteractable currentInteractble, ref List<MessageBasedInteractable> listedInteractables, int parentKeyValue
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

        foreach(MessageBasedInteractable child in currentInteractble.GetComponent<MessageBasedInteractable>().children)
        {
            initialiseLookupData(child, ref listedInteractables, parentKeyValue);
        }
    }

    /// <summary>
    /// method for toggling whether or not this interactable is locked or unowned.
    /// </summary>
    /// <param name="isLockRequest"></param>
    public void toggleOwnershipLockout(bool isLockRequest)
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

    [Obsolete]
    /// <summary>
    /// depth first recursive method for changing the lock status of all interactables starting from the current
    /// interactable.
    /// </summary>
    /// <param name="currentInteractble"></param>
    /// <param name="isLockRequest"></param>
    public void toggleOwnershipLockoutRecursive(MessageBasedInteractable currentInteractble, bool isLockRequest)
    {
        //call the toggle lockout method from the currentINteractable
        currentInteractble.toggleOwnershipLockout(isLockRequest);

        //interate over the interactbles children and recursively call this method
        foreach (MessageBasedInteractable child in currentInteractble.GetComponent<MessageBasedInteractable>().children)
        {
            toggleOwnershipLockoutRecursive(child, isLockRequest);
        }
    } 
}