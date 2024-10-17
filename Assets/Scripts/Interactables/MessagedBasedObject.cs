using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MessageBasedManipulator))]
public class MessagedBasedObject : NetworkInteractable
{
    public MessageBasedManipulator manipulator;
    
    protected void Awake()
    {
        //initialse the manipulator to the one that should be attached to this object
        manipulator = GetComponent<MessageBasedManipulator>();
        //subscribe to select and deselect events
        manipulator.selectEntered.AddListener(onSelection);
        manipulator.selectExited.AddListener(onDeselection);
    }

    private void onSelection(SelectEnterEventArgs args)
    {
        //request ownership of this object
        requestOwnership();
    }

    private void onDeselection(SelectExitEventArgs args)
    {
        //signal the server to reset ownership of this object
        revokeOwnership();
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
        //set positon and rotation
        transform.SetPositionAndRotation(newPosition,newRotation);
        //set local scale
        transform.localScale = newScale;
    }

    public override void initialiseLookupData
    ( 
        NetworkInteractable currentInteractble, 
        ref List<NetworkInteractable> listedInteractables, 
        int parentKeyValue
    )
    {
        //add this network interactable to the listed Interactables list
        listedInteractables.Add(currentInteractble);

        //initialise this network interactables lookupdata 
        currentInteractble.lookupData = new NetworkInteractableLookupData
        {
            //set the root key to the parent key value
            parentKey = parentKeyValue,
            //set this network interactables object index as 0 as we dont use this method recursively here.
            objectIndex = 0,
        };
    }

    public override void toggleOwnershipLockoutRecursive(NetworkInteractable currentInteractble, bool isLockRequest)
    {
        
    }


    public override void toggleOwnershipLockout(bool isLockRequest)
    {
        if (isLockRequest)
        {
            //set the ownership id of the current interactable to locked
            setOwnerID(GlobalConstants.OWNERSHIP_LOCKED);
            return;
        }
        //set the ownership id of the current interactable to unowned
        setOwnerID(GlobalConstants.OWNERSHIP_UNOWNED);
    }
}
