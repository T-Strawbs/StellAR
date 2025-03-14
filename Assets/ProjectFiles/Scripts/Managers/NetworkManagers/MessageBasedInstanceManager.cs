﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// this class manages the state of all active instances of MessageBased Interactables over the network and ensures
/// that they remain synchronised across all clients. This manager handles the ownership and translations of 
/// MessageBased interactables
/// </summary>
public class MessageBasedInstanceManager : Singleton<MessageBasedInstanceManager>, StartupProcess
{
    /// <summary>
    /// Dictionary for holding a reference for each instantiated model prefab. 
    /// It contains a key used as an identifier for each model and is paired with a List
    /// of every gameobject within that model's object tree. Essentially we have a list
    /// where we have flattened the model's tree structure so we can use a parent key
    /// to find the target model and an object index to find a given object from that 
    /// target model to apply mutations to it all at a time complexity of O(1). 
    /// </summary>
    private Dictionary<int, List<MessageBasedInteractable>> registeredInteractbleLookUp { get; set; } = new Dictionary<int, List<MessageBasedInteractable>>();

    /// <summary>
    /// the current value of the registeredInteractbleLookUp's key. We increment this
    /// each time a model prefab has been instantiated and registered with the MessageBasedInstanceManager.
    /// </summary>
    private int nextKeyValue = 0;

    /// <summary>
    /// Dictionary that pairs a set of network interactables that each client owns with the key of the client ID.
    /// Used to keep track of which client currently owns what object
    /// </summary>
    private Dictionary<ulong, HashSet<MessageBasedInteractable>> currentlyOwnedInteractablesLookUp { get; set; } = new Dictionary<ulong, HashSet<MessageBasedInteractable>>();

    private void Awake()
    {
        ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
    }

    public void onStartupProcess()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            //if we're not the server we leave
            if (!NetworkManager.Singleton.IsServer)
                return;

            registerMessages();

            //insert the client id of the host into the currentlyOwnedInteractablesLookUp and initialise the list
            currentlyOwnedInteractablesLookUp[NetworkManager.Singleton.LocalClientId] = new HashSet<MessageBasedInteractable>();

        };
        //Register lambda event that registers the messages of this manager on the most recent clients end
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) =>
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId == clientID)
                registerMessages();

            if (NetworkManager.Singleton.IsServer)
            {
                //insert the new client id into the currentlyOwnedInteractablesLookUp and initialise the list
                currentlyOwnedInteractablesLookUp[clientID] = new HashSet<MessageBasedInteractable>();
            }
        };
        //Register  event for ensuring that when a client disconnects they dont lock the ownership and 
        //manipulability of any object they controlled
        NetworkManager.Singleton.OnClientDisconnectCallback += handleClientDisconnect;
    }

    /// <summary>
    /// Method for registering this class' custom messages with the NGO NetworkManager.
    /// </summary>
    private void registerMessages()
    {
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("networkInteractableOwnershipServerRequest", networkInteractableOwnershipServerRequest);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("networkInteractableOwnershipClientBroadcast", networkInteractableOwnershipClientBroadcast);

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("updateNetworkInteractableTransformServerRequest", updateNetworkInteractableTransformServerRequest);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("updateNetworkInteractableTransformClientBroadcast", updateNetworkInteractableTransformClientBroadcast);
    }


    #region General Methods
    /// <summary>
    /// Method for registering a prefab instance of a messagebased interactable (networked model)
    /// </summary>
    /// <param name="networkInteractable"></param>
    public void registerNetworkInteractable(MessageBasedInteractable networkInteractable)
    {
        //create a new list
        List<MessageBasedInteractable> interactableList = new List<MessageBasedInteractable>();

        //add it to the interactable map at using the next key value as key
        registeredInteractbleLookUp[nextKeyValue] = interactableList;

        //recursively initialise the lookupData for the networkwork interactble
        networkInteractable.initialiseLookupData(networkInteractable,ref interactableList,nextKeyValue);

        //increment the next key value
        nextKeyValue++;

    }

    /// <summary>
    /// search method for finding a target interactable via its parent key and object index.
    /// </summary>
    /// <param name="parentKey"></param>
    /// <param name="objectIndex"></param>
    /// <returns></returns>
    public MessageBasedInteractable lookupNetworkInteractable(int parentKey, int objectIndex)
    {
        MessageBasedInteractable networkInteractable = null;
        
        try
        {
            networkInteractable = registeredInteractbleLookUp[parentKey][objectIndex];
        }
        catch(Exception e)
        {
            DebugConsole.Instance.LogError($"instance manager cannot find an object at key:{parentKey} Index: {objectIndex}");
        }

        return networkInteractable;
    }

    /// <summary>
    /// method for handling the ownership of an interactable when a client disconnects.
    /// Used to prevent the persistant ownership of multiple interactables which are 
    /// owned by a disconnected client
    /// </summary>
    /// <param name="clientID"></param>
    private void handleClientDisconnect(ulong clientID)
    {
        //if we're not the server then we dont want to execute the following code
        if (!NetworkManager.Singleton.IsServer)
            return;

        //check if the client id is in the currentlyOwnedInteractablesLookUp
        if(!currentlyOwnedInteractablesLookUp.ContainsKey(clientID))
        {
            //it doesnt so return
            DebugConsole.Instance.LogDebug($"the currentlyOwnedInteractablesLookUp doenst contain the key of clientid:{clientID}");
            return;
        }

        //check if the the client had ownership of any object
        foreach (MessageBasedInteractable networkInteractable in currentlyOwnedInteractablesLookUp[clientID])
        {
            //release ownership
            expressNetworkInteractableOwnershipRevokationRequest(networkInteractable);
            DebugConsole.Instance.LogDebug($"Client({clientID}) ownership of {networkInteractable.name} shouldve been revoked ");

        }
        //if the set exists at the key of client ID
        if(currentlyOwnedInteractablesLookUp[clientID] != null)
        {
            //clear the set
            currentlyOwnedInteractablesLookUp[clientID].Clear();
            //remove the key
            currentlyOwnedInteractablesLookUp.Remove(clientID);
        }

        DebugConsole.Instance.LogDebug($"Client({clientID}) ownership of objects shouldve been revoked ");
    }

    #endregion General Methods

    #region Messaging    
    /// <summary>
    /// Invoker method that begins the requesting process for a client to gain ownership of an object
    /// over the network.
    /// </summary>
    /// <param name="networkInteractable"></param>
    public void requestOwnershipOfNetworkInteractable(MessageBasedInteractable networkInteractable)
    {
        //check if the interactable is locked
        if(networkInteractable.isLocked())
        {
            DebugConsole.Instance.LogDebug
                (
                    $"client({NetworkManager.Singleton.LocalClientId}) requested ownership of {networkInteractable.name} " +
                    $"but its locked."
                );
            return;
        }

        //check if the local client already owns the network interactable
        if (networkInteractable.isOwnedByLocalClient())
        {
            DebugConsole.Instance.LogDebug(
                $"Client({NetworkManager.Singleton.LocalClientId}) tried to request ownership of {networkInteractable.name}" +
                $" but they alreay have ownership.");
            //we already own the object so return
            return; 
        }
            
        //check if the network interactable is currently owned by someone else
        if (networkInteractable.isOwned())
        {
            DebugConsole.Instance.LogDebug(
                $"Client({NetworkManager.Singleton.LocalClientId}) tried to request ownership of {networkInteractable.name}" +
                $" but its owned by client({networkInteractable.getOwnerID()})");
            //someone else already owns the network interactable
            return;
        }

        //we should be good to send a request to the server to grant us (local client) ownership

        //create the NetworkInteractableOwnershipRequest struct
        NetworkInteractableOwnershipRequest ownershipRequest = new NetworkInteractableOwnershipRequest 
        {
            //set the parent key from the network interactables lookup data
            parentKey = networkInteractable.lookupData.parentKey,
            //set the object index from the network interactables lookup data
            objectIndex = networkInteractable.lookupData.objectIndex,
            //this is not a revoke request so we set the flag to false
            isRevokeRequest = false,
            //assign the id of the local client as the new owner
            newOwnerID = NetworkManager.Singleton.LocalClientId,
        };

        //create the writer that will pack our message payload to the byte size of the lookup data
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(ownershipRequest),Allocator.Temp);

        using (writer)
        {
            //write the lookup data to the writer
            writer.WriteValueSafe(ownershipRequest);
            //send the request to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    //The message name the custom message manager will direct this message to
                    "networkInteractableOwnershipServerRequest",
                    //The ID of the message recipient -- in this case we are sending it to the server
                    NetworkManager.ServerClientId,
                    //inject the payload
                    writer,
                    //The reliabiltiy of the message delivery - Reliable makes it so the recipient is 
                    //practically guaranteed to recieve the message and Unreliable makes it faster to 
                    //send and receive but theres no guarantee to that it will reach anyone
                    NetworkDelivery.Reliable
                );
        }

    }

    /// <summary>
    /// method for requesting the server to revoke the ownership of a target interactable that is
    /// owned by this client.
    /// </summary>
    /// <param name="networkInteractable"></param>
    public void revokeOwnershipOfNetworkInteractable(MessageBasedInteractable networkInteractable)
    {
        //check if the local client owns the network interactable
        if (!networkInteractable.isOwnedByLocalClient())
        {
            DebugConsole.Instance.LogDebug(
                $"Client({NetworkManager.Singleton.LocalClientId}) tried to revoke ownership of {networkInteractable.name}" +
                $" but they dont have ownership client({networkInteractable.getOwnerID()}) does.");
            //we dont own the object so return
            return;
        }

        //create the NetworkInteractableOwnershipRequest struct
        NetworkInteractableOwnershipRequest ownershipRequest = new NetworkInteractableOwnershipRequest
        {
            //set the parent key from the network interactables lookup data
            parentKey = networkInteractable.lookupData.parentKey,
            //set the object index from the network interactables lookup data
            objectIndex = networkInteractable.lookupData.objectIndex,
            //this is not a revoke request so we set the flag to false
            isRevokeRequest = true,
            //assign the new owner ID to our unowned value as no one owns it
            newOwnerID = GlobalConstants.OWNERSHIP_UNOWNED,
        };

        //create the writer that will pack our message payload to the byte size of the lookup data
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(ownershipRequest), Allocator.Temp);

        using (writer)
        {
            //write the lookup data to the writer
            writer.WriteValueSafe(ownershipRequest);
            //send the request to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    //The message name the custom message manager will direct this message to
                    "networkInteractableOwnershipServerRequest",
                    //The ID of the message recipient -- in this case we are sending it to the server
                    NetworkManager.ServerClientId,
                    //inject the payload
                    writer,
                    //The reliabiltiy of the message delivery - Reliable makes it so the recipient is 
                    //practically guaranteed to recieve the message and Unreliable makes it faster to 
                    //send and receive but theres no guarantee to that it will reach anyone
                    NetworkDelivery.Reliable
                );
        }
    }

    /// <summary>
    /// message for requesting the server to handle the ownership state of a given interactable. 
    /// Either to grant or revoke ownership of the interactable.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void networkInteractableOwnershipServerRequest(ulong senderID, FastBufferReader messagePayload)
    {
        //we have to initialise a lookup data struct again so we can repack it into a new payload to send out
        NetworkInteractableOwnershipRequest ownershipRequest;

        //deserialise the struct from bytes to an object
        messagePayload.ReadValueSafe(out ownershipRequest);
        
        List<ulong> recipientIDs = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        //remove the servers/hosts local client id from the recipients as we are going to set ownership locally
        //before broadcasting the request to the other clients
        recipientIDs.Remove(NetworkManager.Singleton.LocalClientId);

        //attempt to grab the target network interactable
        MessageBasedInteractable targetInteractable = lookupNetworkInteractable(ownershipRequest.parentKey, ownershipRequest.objectIndex);

        //check if the target exists
        if(targetInteractable == null)
        {
            DebugConsole.Instance.LogDebug($"client({senderID}) tried to request ownership of an object that doesnt exist.");
            //it shouldnt be null on server side so we need to return
            return;
        }
        //check if ths was an ownership revoke request
        if(ownershipRequest.isRevokeRequest)
        {
            //attempt to remove the target interactable from the owned interactables lookup
            currentlyOwnedInteractablesLookUp[targetInteractable.getOwnerID()].Remove(targetInteractable);
        }
        else
        {
            //attempt to add the target interactable to the owned interactables lookup for the new owner
            currentlyOwnedInteractablesLookUp[ownershipRequest.newOwnerID].Add(targetInteractable);
        }

        //set the target interactables owner id to the one from the message
        targetInteractable.setOwnerID(ownershipRequest.newOwnerID);

        //create a new writer
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(ownershipRequest), Allocator.Temp);

        using (writer)
        {
            //pack the payload
            writer.WriteValueSafe(ownershipRequest);

            //send the message
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                //The message name the custom message manager will direct this message to
                "networkInteractableOwnershipClientBroadcast",
                //the id/s of the recipient/s
                recipientIDs,
                //inject the payload
                writer,
                //The reliabiltiy of the message delivery - Reliable makes it so the recipient is 
                //practically guaranteed to recieve the message and Unreliable makes it faster to 
                //send and receive but theres no guarantee to that it will reach anyone
                NetworkDelivery.Reliable
                );
        }

    }

    /// <summary>
    /// Serverside method that broadcasts to all clients to revoke the ownership of an interactable that
    /// was owned by a disconnected client.
    /// </summary>
    /// <param name="targetInteractable"></param>
    private void expressNetworkInteractableOwnershipRevokationRequest(MessageBasedInteractable targetInteractable)
    {

        NetworkInteractableOwnershipRequest ownershipRequest = new NetworkInteractableOwnershipRequest
        {
            //set the parent key from the network interactables lookup data
            parentKey = targetInteractable.lookupData.parentKey,
            //set the object index from the network interactables lookup data
            objectIndex = targetInteractable.lookupData.objectIndex,
            //this is not a revoke request so we set the flag to false
            isRevokeRequest = true,
            //assign the new owner ID to our unowned value as no one owns it
            newOwnerID = GlobalConstants.OWNERSHIP_UNOWNED,
        };

        //set the target interactables owner id to the one from the message
        targetInteractable.setOwnerID(GlobalConstants.OWNERSHIP_UNOWNED);

        //create a new writer
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(ownershipRequest), Allocator.Temp);

        List<ulong> recipientIDs = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        //remove the servers/hosts local client id from the recipients as we are going to set ownership locally
        //before broadcasting the request to the other clients
        recipientIDs.Remove(NetworkManager.Singleton.LocalClientId);

        using (writer)
        {
            //pack the payload
            writer.WriteValueSafe(ownershipRequest);

            //send the message
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                //The message name the custom message manager will direct this message to
                "networkInteractableOwnershipClientBroadcast",
                //the id/s of the recipient/s
                recipientIDs,
                //inject the payload
                writer,
                //The reliabiltiy of the message delivery - Reliable makes it so the recipient is 
                //practically guaranteed to recieve the message and Unreliable makes it faster to 
                //send and receive but theres no guarantee to that it will reach anyone
                NetworkDelivery.Reliable
                );
        }
    }

    /// <summary>
    /// message for broadcasting to all clients to change a given interactable's ownership status locally on
    /// their end.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void networkInteractableOwnershipClientBroadcast(ulong senderID, FastBufferReader messagePayload)
    {
        //initialise a var to hold the lookup data we've just received
        NetworkInteractableOwnershipRequest ownershipRequest;

        //deserialise the struct from bytes to an object
        messagePayload.ReadValueSafe(out ownershipRequest);

        //lookup the network interactable in the interactble map
        MessageBasedInteractable targetInteractable = lookupNetworkInteractable(ownershipRequest.parentKey,ownershipRequest.objectIndex);

        if(!targetInteractable)
        {
            DebugConsole.Instance.LogError($"client({NetworkManager.Singleton.LocalClientId}) couldnt find the network interactable of " +
                $"parent key : ({ownershipRequest.parentKey}) object index : ({ownershipRequest.objectIndex})");
            return;
        }
        ulong previousID = targetInteractable.getOwnerID();

        //set the target interactables owner id to the one from the message
        targetInteractable.setOwnerID(ownershipRequest.newOwnerID);

        DebugConsole.Instance.LogDebug($"Client({senderID}) changed the ownershipID from client({previousID}) to client({targetInteractable.getOwnerID()})");
    }

    /// <summary>
    /// Method for starting the requesting process for updating the transform of a given interactable over
    /// the network.
    /// </summary>
    /// <param name="networkInteractable"></param>
    /// <param name="targetPosition"></param>
    /// <param name="targetRotation"></param>
    /// <param name="targetScale"></param>
    public void requestUpdateNetworkInteractableTransform(
        MessageBasedInteractable networkInteractable, 
        Vector3 targetPosition,
        Quaternion targetRotation,
        Vector3 targetScale)
    {
        //check if the interactable is locked
        if(networkInteractable.isLocked())
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) attempted to request" +
                $" the server to update the {networkInteractable.name}'s transform but its locked");
            return;
        }

        //check if the local client has ownership of the network interactable
        if(!networkInteractable.isOwnedByLocalClient())
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) attempted to request" +
                $" the server to update the {networkInteractable.name}'s transform but " +
                $"client({networkInteractable.getOwnerID()}) owns it");
            return;
        }

        //we should apply the transform on the local client first then send the update across the network
        networkInteractable.updateTransformLocalClient(targetPosition, targetRotation, targetScale);

        //create the transform request data for us to send to the server
        NetworkInteractableUpdateTransformRequest transformRequest = new NetworkInteractableUpdateTransformRequest()
        {
            //assign the parent key from the network interactables lookup data
            parentKey = networkInteractable.lookupData.parentKey,
            //assign the object index from the network interactables lookup data
            objectIndex = networkInteractable.lookupData.objectIndex,
            //assign the target position
            targetPosition = networkInteractable.transform.localPosition,
            //assign the target rotation
            targetRotation = networkInteractable.transform.localRotation,
            //assing the target scale
            targetScale = targetScale
        };
        //create the writer that will pack our message payload at a byte size of our transform request
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(transformRequest), Allocator.Temp);

        using (writer)
        {
            //pack our message payload
            writer.WriteValueSafe(transformRequest);

            //send message to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "updateNetworkInteractableTransformServerRequest",
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.Unreliable
                );
        }
    }

    /// <summary>
    /// message for requesting the server to make a client broadcast for them to update the transform
    /// of a given interactable on their end.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void updateNetworkInteractableTransformServerRequest(ulong senderID, FastBufferReader messagePayload)
    {
        //initialise the tranform request var for us to send it to all clients later
        NetworkInteractableUpdateTransformRequest transformRequest;

        //unpack the message by deserialising from bytes to an object
        messagePayload.ReadValueSafe(out transformRequest);

        //create a new writer to pack the transform request of a byte size that matches the transform request
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(transformRequest), Allocator.Temp);

        //generate a the list of client recipients
        List<ulong> recipients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds.ToArray());
        //remove the sender id
        recipients.Remove(senderID);

        using (writer)
        {
            //pack message payload
            writer.WriteValueSafe(transformRequest);

            //send message to all clients
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "updateNetworkInteractableTransformClientBroadcast",
                    recipients,
                    writer,
                    NetworkDelivery.Unreliable
                );
        }
    }

    /// <summary>
    /// message that is broadcasted to all clients to update the transform of a given interactable locally 
    /// on their end.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void updateNetworkInteractableTransformClientBroadcast(ulong senderID, FastBufferReader messagePayload)
    {
        //initialise the tranform request var
        NetworkInteractableUpdateTransformRequest transformRequest;

        //unpack the message by deserialising from bytes to an object
        messagePayload.ReadValueSafe(out transformRequest);

        //find the target network interactable
        MessageBasedInteractable targetInteractable = lookupNetworkInteractable(transformRequest.parentKey, transformRequest.objectIndex);

        if (!targetInteractable)
        {
            DebugConsole.Instance.LogError($"client({NetworkManager.Singleton.LocalClientId}) couldnt find the network interactable of " +
                $"parent key : ({transformRequest.parentKey}) object index : ({transformRequest.objectIndex})");
            return;
        }

        //apply the tranform 
        targetInteractable.updateTransformLocalClientLocalPosition
            (
                transformRequest.targetPosition,transformRequest.targetRotation,transformRequest.targetScale
            );
    }
    #endregion Messaging

}

/// <summary>
/// Data struct used a message payload for requesting the ownership of an interactable to be changed.
/// </summary>
public struct NetworkInteractableOwnershipRequest : INetworkSerializable
{
    /// <summary>
    /// The name of the root object for this interactable
    /// </summary>
    public int parentKey;
    /// <summary>
    /// The name of this interactable
    /// </summary>
    public int objectIndex;
    /// <summary>
    /// The flag for determining whether is request is for revoking the 
    /// ownership of the object or not
    /// </summary>
    public bool isRevokeRequest;
    /// <summary>
    /// The client id of the new owner
    /// </summary>
    public ulong newOwnerID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);
        serializer.SerializeValue(ref isRevokeRequest);
        serializer.SerializeValue(ref newOwnerID);
    }
}
/// <summary>
/// Data struct used a message payload for requesting that the tranform for a target interactable is 
/// updated.
/// </summary>
public struct NetworkInteractableUpdateTransformRequest : INetworkSerializable
{
    /// <summary>
    /// The name of the root object for this interactable
    /// </summary>
    public int parentKey;
    /// <summary>
    /// The name of this interactable
    /// </summary>
    public int objectIndex; 
    /// <summary>
    /// the target position we want the transform to be updated too.
    /// </summary>
    public Vector3 targetPosition;
    /// <summary>
    /// the target rotation we want the transform to be updated too.
    /// </summary>
    public Quaternion targetRotation;
    /// <summary>
    /// the target scale we want the transform to be updated too.
    /// </summary>
    public Vector3 targetScale;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);

        serializer.SerializeValue(ref targetPosition);
        serializer.SerializeValue(ref targetRotation);
        serializer.SerializeValue(ref targetScale);
    }
}

