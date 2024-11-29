using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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
/// Manager class for handling the explosion process of interactables across the network.
/// </summary>
public class MessageBasedExplodableHandler : Singleton<MessageBasedExplodableHandler>, StartupProcess
{
    private void Awake()
    {
        ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
    }

    public void onStartupProcess()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            if (NetworkManager.Singleton.IsServer)
                registerMessages();
        };
        //Register lambda event that executes when the a client connects to the server.
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) =>
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId == clientID)
                registerMessages();
        };
    }

    /// <summary>
    /// Method for registering this class' custom messages with the NGO NetworkManager.
    /// </summary>
    private void registerMessages()
    {
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler
            ("interactableExplosionServerRequest", interactableExplosionServerRequest);


        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler
            ("interactableExplosionClientBroadcast", interactableExplosionClientBroadcast);


        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler
            ("interactableCollaseServerRequest", interactableCollaseServerRequest);

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler
            ("interactableCollaseClientBroadcast", interactableCollaseClientBroadcast);
    }

    /// <summary>
    /// method for beginning the process of exploding by requesting the server to 
    /// explode the given interactable.
    /// </summary>
    /// <param name="interactable"></param>
    public void requestInteractableExplostion(MessageBasedInteractable interactable)
    {
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is preparing an explosion request to the server.");
         
        //check if the interactable is locked
        if (interactable.isLocked())
        {
            DebugConsole.Instance.LogDebug
                (
                    $"client({NetworkManager.Singleton.LocalClientId}) requested ownership of {interactable.name} " +
                    $"but its locked."
                );
            return;
        }

        //check if the interactble is owned by anyone
        if (interactable.isOwned())
        {
            DebugConsole.Instance.LogDebug
                (
                    $"client({NetworkManager.Singleton.LocalClientId}) wants to explode {interactable.name} " +
                    $"but its owned by {interactable.getOwnerID()}"
                );
            return;
        }

        //check if the interactable can explode
        if (!interactable.canExplode())
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) requested to explode {interactable.name} but its not explodable");
            return;
        }

        //start our request interactble explosion coroutine
        StartCoroutine(requestInteractableExplostionCoroutine(interactable));
    }

    /// <summary>
    /// Coroutine for sending an explosion request to the server. Works by first sending a request to the 
    /// server to lock the interactable then awaits for the lock to occur locally then if sucessful,
    /// send the explosion request to the server.
    /// </summary>
    /// <param name="interactable"></param>
    /// <returns></returns>
    private IEnumerator requestInteractableExplostionCoroutine(MessageBasedInteractable interactable)
    {
        //request the MessageBasedInteractable Instance Manager to lockout this interactable across the network
        if(!InteractableLockHandler.Instance.requestInteractableLock(interactable, true))
        {
            //the message didnt get sent as we failed to meet a condition
            yield break;
        }

        //float for tracking the current time that we use to check if we have timedout
        float timeoutDuration = 0f; ;

        //while our interactable is not locked and we havent timedout
        while(!interactable.isLocked() && timeoutDuration < GlobalConstants.MAX_WAIT_TIME)
        {
            //wait for the next frame to check again
            yield return new WaitForEndOfFrame();
            //increment the time we've taken to wait for the lock to occur
            timeoutDuration += Time.deltaTime;
        }

        //check if the lockout was successful
        if (!interactable.isLocked())
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) requested to explode {interactable.name} but the lock was unsuccessful." +
                $"\n{interactable.name} is owned by client({interactable.getOwnerID()})");
            //it wasn't successful so break out of the corountine
            yield break;
        }

        //At this stage we should be good to request the server to initialise the explosion of the interactable

        //create the explosion request
        InteractableExplodeRequest explodeRequest = new InteractableExplodeRequest()
        {
            parentKey = interactable.lookupData.parentKey,
            objectIndex = interactable.lookupData.objectIndex
        };

        //create the writer that will pack our payload into the message
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(explodeRequest), Allocator.Temp);

        using (writer)
        {
            //pack the payload
            writer.WriteValueSafe(explodeRequest);

            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is sending an explosion request to the server.");

            //send the message to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "interactableExplosionServerRequest", NetworkManager.ServerClientId, writer, NetworkDelivery.Reliable 
                );
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) shouldve sent an explosion request to the server.");
        }
        
    }

    /// <summary>
    /// message for requesting the server to broadcast the explosion request to all clients.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void interactableExplosionServerRequest(ulong senderID, FastBufferReader messagePayload)
    {
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId})/server received message");
        
        InteractableExplodeRequest explodeRequest;
        //unpack the payload to get our request data
        messagePayload.ReadValueSafe(out explodeRequest);

        //create the writer that will pack our payload into the message
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(explodeRequest), Allocator.Temp);

        using (writer)
        {
            //pack our new payload with the explosion request
            writer.WriteValueSafe(explodeRequest);

            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) sending message to all clients");

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll
                (
                    "interactableExplosionClientBroadcast", writer, NetworkDelivery.Reliable
                );
        }
    }

    /// <summary>
    /// message for broadcasting to all clients to explode the target interactable locally on their end.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void interactableExplosionClientBroadcast(ulong senderID, FastBufferReader messagePayload)
    {
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) receiveed the message");
        InteractableExplodeRequest explodeRequest;

        //unpack our request data
        messagePayload.ReadValueSafe(out explodeRequest);

        //try to grab our target interactable from the Instance Manager's lookup table
        MessageBasedInteractable targetInteractable = (MessageBasedInteractable) MessageBasedInstanceManager.Instance.lookupNetworkInteractable
            (
                explodeRequest.parentKey, explodeRequest.objectIndex
            );
        //check if we found our target interactable
        if (!targetInteractable)
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) couldnt find the target");
            return;
        }

        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) should be exploding {targetInteractable.name}");

        //we should now be able to explode
        targetInteractable.explodeInteractable();

        if(NetworkManager.Singleton.IsServer)
            //tell the server to unlock the interactable as we have finished exploding
            InteractableLockHandler.Instance.requestInteractableLock(targetInteractable, false);
    }

    /// <summary>
    /// method for beginning the collapsing process of the given across the network.
    /// </summary>
    /// <param name="interactable"></param>
    /// <param name="isSingleCollapse"></param>
    public void requestInteractableCollapse(MessageBasedInteractable interactable, bool isSingleCollapse)
    {
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is preparing a collapse request to the server.");
        //check if the interactable is locked
        if (interactable.isLocked())
        {
            DebugConsole.Instance.LogDebug
                (
                    $"client({NetworkManager.Singleton.LocalClientId}) requested  the collapse of {interactable.name} " +
                    $"but its locked."
                );
            return;
        }

        //check if the interactble is owned by anyone
        if (interactable.isOwned())
        {
            DebugConsole.Instance.LogDebug
                (
                    $"client({NetworkManager.Singleton.LocalClientId}) wants to collapse {interactable.name} " +
                    $"but its owned by {interactable.getOwnerID()}"
                );
            return;
        }

        //check if we're collapsing all levels of an interactable or only one
        if(isSingleCollapse)
        {
            //check if we can collapse one level
            if(!interactable.areCollapsablesOwnedSingle())
            {
                DebugConsole.Instance.LogDebug(
                    $"client({NetworkManager.Singleton.LocalClientId})" +
                    $" requested that {interactable.name} would collapse one level but it cannot.");
                return;
            }
        }
        else
        {
            //check if we can collapse all levels of the interactable
            if (!interactable.areCollapsablesOwnedAll())
            {
                DebugConsole.Instance.LogDebug(
                    $"client({NetworkManager.Singleton.LocalClientId})" +
                    $" requested that {interactable.name} would collapse all levels but it cannot.");
                return;
            }
        }

        //start our request interactble collapse coroutine
        StartCoroutine(interactableCollaseServerRequest(interactable,isSingleCollapse));
    }

    /// <summary>
    /// Coroutine for sending a collapse request for the target interactable to the server.
    /// Works by initially sending a lock request to the server to lock the interactable across 
    /// the network and then waiting for the lock to occur. Then if the lock is successfully applied,
    /// then we send the collapse request to the server.
    /// </summary>
    /// <param name="interactable"></param>
    /// <param name="isSingleCollapse"></param>
    /// <returns></returns>
    private IEnumerator interactableCollaseServerRequest(MessageBasedInteractable interactable, bool isSingleCollapse)
    {
        //request that each iteractable that is involved in collapsing is locked.
        //this is to ensure that when the collapsing process is occuring that no
        //client can gain or have ownership of any of the involved interactables
        //otherwise we risk a desync.
        List<MessageBasedInteractable> lockedInteractables = InteractableLockHandler.Instance.
            requestCollapsingInteractableLockEngage(interactable, isSingleCollapse);

        //if the list of locked interactables doesnt exist we break out of the corountine
        if(lockedInteractables == null)
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) requested that {interactable.name}" +
                $" should be locked before its collapsed but one of the collapsables were already locked.");
            yield break;
        }

        //setup a flag to ensure that all lockable interactables are locked
        int lockedCount = 0, loop = 0;
        //initialise the timeout duration to 0
        float timeoutDuration = 0f;
        //while not all interactables are locked and we havent exceeded our max wait time
        while(lockedCount != lockedInteractables.Count && timeoutDuration < GlobalConstants.MAX_WAIT_TIME)
        {
            //Debug.Log($"lock check loop({loop})");
            //iterate over the locked interactables 
            foreach (MessageBasedInteractable lockedInteractable in lockedInteractables)
            {
                //check if the interactable is locked and if true increment our lockcount
                if (lockedInteractable.isLocked())
                {
                    lockedCount++;

                    //Debug.Log($"{lockedInteractable.name} was locked");
                }
                else
                {
                    //Debug.Log($"{lockedInteractable.name} hasnt been locked");
                }
            }
            if (lockedCount != lockedInteractables.Count)
            {
                lockedCount = 0;
                //Debug.Log($"not all interactables were locked.");
            }
            yield return new WaitForEndOfFrame();
            timeoutDuration += Time.deltaTime;
            loop++;
        }

        //if not all lock were set then we break out of this method
        if(lockedCount != lockedInteractables.Count)
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) " +
                $"requested that {interactable.name} would collapse but not all locks were set.");
            yield break;
        }

        //since all locks were set we should send an collape request to the server
        Debug.Log($"____ WE CAN COLLAPSE FROM {interactable.name} _____");

        //create a new collapse request object
        InteractableCollapseRequest collapseRequest = new InteractableCollapseRequest()
        {
            parentKey = interactable.lookupData.parentKey,
            objectIndex = interactable.lookupData.objectIndex,
            isSingleCollapse = isSingleCollapse
        };

        //create the write to pack the message payload
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(collapseRequest), Allocator.Temp);

        using (writer)
        {
            //pack our payload
            writer.WriteValueSafe(collapseRequest);

            //send our request to the server

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "interactableCollaseServerRequest", NetworkManager.ServerClientId,writer,NetworkDelivery.Reliable
                );
        }

    }

    /// <summary>
    /// message for requesting the server to broadcast a collapse request to all clients
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void interactableCollaseServerRequest(ulong senderID, FastBufferReader messagePayload)
    {
        InteractableCollapseRequest collapseRequest;

        //unpack the collapse request
        messagePayload.ReadValueSafe(out collapseRequest);

        //create a new write to pack our message payload
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(collapseRequest), Allocator.Temp);

        using (writer)
        {
            //pack our collapse request into the new payload
            writer.WriteValueSafe(collapseRequest);

            //send the request
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll
                (
                    "interactableCollaseClientBroadcast",writer,NetworkDelivery.Reliable
                );
        }
    }

    /// <summary>
    /// message for broadcasting to all clients to handle the collapse process locally on their end.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void interactableCollaseClientBroadcast(ulong senderID, FastBufferReader messagePayload)
    {
        InteractableCollapseRequest collapseRequest;

        //unpack the request data
        messagePayload.ReadValueSafe(out collapseRequest);

        //attempt to find the target interactable using the Instance Manager's lookup table
        MessageBasedInteractable targetInteractable = (MessageBasedInteractable)MessageBasedInstanceManager.Instance.lookupNetworkInteractable
            (
                collapseRequest.parentKey, collapseRequest.objectIndex
            );
        //check if we found the target interactable
        if (!targetInteractable)
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) couldnt find the target");
            return;
        }

        //collapse the interactable
        targetInteractable.collapseInteractable(collapseRequest.isSingleCollapse);

        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) should be collapsing {targetInteractable.name}");

        //init the lockout flag as false as we want to unlock all interactables that were locked in the process
        bool isLockoutRequest = false;

        //unlock the target interactable by sending a request to the server.
        InteractableLockHandler.Instance.requestInteractableLockCollapseDisengage(targetInteractable, isLockoutRequest);
            
    }
}

/// <summary>
/// serialisable struct for explosion request data
/// </summary>
public struct InteractableExplodeRequest : INetworkSerializable
{
    //the parent key of the interactable
    public int parentKey;
    //the object index of the interactable
    public int objectIndex;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);
    }
}
/// <summary>
/// serialisable struct for collapse request data
/// </summary>
public struct InteractableCollapseRequest : INetworkSerializable
{
    //the parent key of the interactable
    public int parentKey;
    //the object index of the interactable
    public int objectIndex;
    //flag for determining if this is a request for a single or complete collapse.
    public bool isSingleCollapse;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);
        serializer.SerializeValue(ref isSingleCollapse);
    }
}
