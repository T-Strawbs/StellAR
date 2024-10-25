using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MessageBasedExplodableHandler : Singleton<MessageBasedExplodableHandler>, CustomMessageHandler
{
    private void Awake()
    {
        ApplicationManager.Instance.onProcessCustomMessengers.AddListener(registerNetworkEventListeners);
    }

    public void registerNetworkEventListeners()
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

    public void registerMessages()
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

        StartCoroutine(requestInteractableExplostionCoroutine(interactable));
    }

    private IEnumerator requestInteractableExplostionCoroutine(MessageBasedInteractable interactable)
    {
        //request the MessageBasedInteractable Instance Manager to lockout this interactable across the network
        if(!InteractableLockHandler.Instance.requestInteractableLock(interactable, true))
        {
            //the message didnt get sent as we failed to meet a condition
            yield break;
        }

        float timeoutDuration = 0f; ;

        while(!interactable.isLocked() && timeoutDuration < GlobalConstants.MAX_WAIT_TIME)
        {
            yield return new WaitForEndOfFrame();
            timeoutDuration += Time.deltaTime;
        }

        //check if the lockout was successful
        if (!interactable.isLocked())
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) requested to explode {interactable.name} but the lock was unsuccessful." +
                $"\n{interactable.name} is owned by client({interactable.getOwnerID()})");
            yield break;
        }

        //we should be good to request the server to initialise the explosion of the interactable

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

    private void interactableExplosionServerRequest(ulong senderID, FastBufferReader messagePayload)
    {
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId})/server received message");
        InteractableExplodeRequest explodeRequest;

        messagePayload.ReadValueSafe(out explodeRequest);

        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(explodeRequest), Allocator.Temp);

        using (writer)
        {
            writer.WriteValueSafe(explodeRequest);

            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) sending message to all clients");

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll
                (
                    "interactableExplosionClientBroadcast", writer, NetworkDelivery.Reliable
                );
        }
    }

    private void interactableExplosionClientBroadcast(ulong senderID, FastBufferReader messagePayload)
    {
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) receiveed the message");
        InteractableExplodeRequest explodeRequest;

        messagePayload.ReadValueSafe(out explodeRequest);

        MessageBasedInteractable targetInteractable = (MessageBasedInteractable) MessageBasedInstanceManager.Instance.lookupNetworkInteractable
            (
                explodeRequest.parentKey, explodeRequest.objectIndex
            );


        if (!targetInteractable)
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) couldnt find the target");
            return;
        }

        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) should be exploding {targetInteractable.name}");

        //we should now be able to explode
        targetInteractable.explodeInteractable();

        if(NetworkManager.Singleton.IsServer)
            //unlock the target interactable
            InteractableLockHandler.Instance.requestInteractableLock(targetInteractable, false);
    }


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

        StartCoroutine(interactableCollaseServerRequest(interactable,isSingleCollapse));
    }

    private IEnumerator interactableCollaseServerRequest(MessageBasedInteractable interactable, bool isSingleCollapse)
    {
        //request that each iteractable which needs to be collapsed be locked
        List<MessageBasedInteractable> lockedInteractables = InteractableLockHandler.Instance.
            requestCollapsingInteractableLockEngage(interactable, isSingleCollapse);

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

    private void interactableCollaseServerRequest(ulong senderID, FastBufferReader messagePayload)
    {
        InteractableCollapseRequest collapseRequest;

        messagePayload.ReadValueSafe(out collapseRequest);

        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(collapseRequest), Allocator.Temp);

        using (writer)
        {
            writer.WriteValueSafe(collapseRequest);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll
                (
                    "interactableCollaseClientBroadcast",writer,NetworkDelivery.Reliable
                );
        }
    }

    private void interactableCollaseClientBroadcast(ulong senderID, FastBufferReader messagePayload)
    {
        InteractableCollapseRequest collapseRequest;

        messagePayload.ReadValueSafe(out collapseRequest);

        MessageBasedInteractable targetInteractable = (MessageBasedInteractable)MessageBasedInstanceManager.Instance.lookupNetworkInteractable
            (
                collapseRequest.parentKey, collapseRequest.objectIndex
            );

        if (!targetInteractable)
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) couldnt find the target");
            return;
        }

        //collapse the interactable
        targetInteractable.collapseInteractable(collapseRequest.isSingleCollapse);

        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) should be collapsing {targetInteractable.name}");

        bool isLockoutRequest = false;

        //unlock the target interactable
        InteractableLockHandler.Instance.requestInteractableLockCollapseDisengage(targetInteractable, isLockoutRequest);
            
    }
}

public struct InteractableExplodeRequest : INetworkSerializable
{
    public int parentKey;
    public int objectIndex;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);
    }
}

public struct InteractableCollapseRequest : INetworkSerializable
{
    public int parentKey;
    public int objectIndex;
    public bool isSingleCollapse;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);
        serializer.SerializeValue(ref isSingleCollapse);
    }
}
