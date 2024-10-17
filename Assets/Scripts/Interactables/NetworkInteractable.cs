using System;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Unity.Netcode;
public abstract class NetworkInteractable : MonoBehaviour
{
    /// <summary>
    /// the id of the client that owns this object
    /// </summary>
    [SerializeField] protected ulong ownerID;

    public NetworkInteractable parent { get; set; }

    public List<Interactable> children = new List<Interactable>();


    public Selectable selectable { get; set; }

    protected virtual void Start()
    {
        //initialise the owner ID as the unowned value
        ownerID = GlobalConstants.OWNERSHIP_UNOWNED;
    }

    /// <summary>
    /// The data struct that references this parent's key and this object's index
    /// in order for the NetworkInteractableInstanceManager to search this object.
    /// </summary>
    public NetworkInteractableLookupData lookupData { get; set; }

    /// <summary>
    /// checks if the networkinteractable is owned by anyone.
    /// </summary>
    /// <returns>
    /// bool: 
    ///     - True:  someone owns the object
    ///     - False: no one owns the object
    /// </returns>
    public virtual bool isOwned()
    {
        if(ownerID != GlobalConstants.OWNERSHIP_UNOWNED && ownerID != GlobalConstants.OWNERSHIP_LOCKED)
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
    public virtual bool isOwnedByLocalCLient()
    {
        if(ownerID == NetworkManager.Singleton.LocalClientId)
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
    public virtual bool isLocked()
    {
        if(ownerID == GlobalConstants.OWNERSHIP_LOCKED)
            return true;
        return false;
    }

    /// <summary>
    /// getter method for this objects owner ID
    /// </summary>
    /// <returns> ulong: The ID of the client that owns this object </returns>
    public virtual ulong getOwnerID()
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
    /// method for requesting the server to give the local client ownership of this object;
    /// </summary>
    public abstract void requestOwnership();

    /// <summary>
    /// method for requesting the server to remove ownership of this object;
    /// </summary>
    public abstract void revokeOwnership();

    /// <summary>
    /// Method for requesting the server to update the transform of this object across all clients
    /// </summary>
    /// <param name="newPosition">The new position that we want updated across the network.</param>
    /// <param name="newRotation">The new rotation that we want updated across the network.</param>
    /// <param name="newScale">The new scale that we want updated across the network.</param>
    public abstract void requestUpdateTransform(Vector3 newPosition, Quaternion newRotation, Vector3 newScale);

    /// <summary>
    /// Method for updating the transform of this NetworkInteracable on the local client's end.
    /// </summary>
    /// <param name="newPosition">The new position that the NetworkInteracables tranform.position should be set to.</param>
    /// <param name="newRotation">The new rotation that the NetworkInteracables tranform.rotation should be set to.</param>
    /// <param name="newScale">The new scale that the NetworkInteracables tranform.scale should be set to.</param>
    public abstract void updateTransformLocalClient(Vector3 newPosition, Quaternion newRotation, Vector3 newScale);

    /// <summary>
    /// Recursive method for initialising the NetworkInteractableLookUpData when the object is registered to the 
    /// NetworkInteractableInstanceManager. Enables us to search for this network interactable over the network.
    /// </summary>
    /// <param name="currentInteractble">
    /// The current NetworkInteractble we're intialising.
    /// </param>
    /// <param name="listedInteractables">
    /// The list of NetworkInteractble objects that would hold a reference to every NetworkInteractable of the Object Tree.
    /// </param>
    /// <param name="parentKeyValue">
    /// the key value of the parent/root object.
    /// </param>
    public abstract void initialiseLookupData(NetworkInteractable currentInteractble, ref List<NetworkInteractable> listedInteractables, int parentKeyValue);

    public abstract void toggleOwnershipLockoutRecursive(NetworkInteractable currentInteractble, bool isLockRequest);

    public abstract void toggleOwnershipLockout(bool isLockRequest);

    
}
