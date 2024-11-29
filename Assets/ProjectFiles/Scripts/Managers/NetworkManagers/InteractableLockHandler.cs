using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class for creating and managing InteractableLocks for locking interactables across the network.
/// </summary>
public class InteractableLockHandler : Singleton<InteractableLockHandler>, StartupProcess
{
    //The interactable lock class is nested and private as this class should not be referenced externally
    #region InteractableLock Definiton
    /// <summary>
    /// class that is used as a lock for a coresponding interactable. Works by engaging the lock then
    /// waiting until the InteractableLockHandler unlocks it or by execeeding the max LOCK_DURATION
    /// then unlocking itself.
    /// </summary>
    private class InteractableLock
    {
        /// <summary>
        /// the ID of this lock
        /// </summary>
        public LockID lockID { get; private set; }  
        /// <summary>
        /// The Locked State of this lock
        /// </summary>
        public bool isLocked { get; private set; } = false;
        /// <summary>
        /// reference to the lock coroutine provided by the InteractableLockHandler.
        /// we store this ref as this lock is not a monobehaviour but our Lock Handler is.
        /// Coroutines can only be used by Unity Objects and using concurrency on Unity
        /// objects isnt allowed. This Allows this C# object to control the coroutine
        /// that is run by the Lock Handler's monobehaviour. Sorcery...
        /// </summary>
        private Coroutine lockCoroutine;
        /// <summary>
        /// The object which owns this lock object. Allows us to not have to inherit all the monobehaviour
        /// contents and that as we just want to keep lock objects lightweight and easily disposed of.
        /// </summary>
        private MonoBehaviour lockOwner;

        /// <summary>
        /// Constructor for InteractableLocks
        /// </summary>
        /// <param name="lockID"></param>
        /// <param name="lockOwner">The Lock Handler as a monobehavior</param>
        public InteractableLock(LockID lockID,MonoBehaviour lockOwner)
        {
            this.lockID = lockID;
            this.lockOwner = lockOwner;
        }

        /// <summary>
        /// method for locking this lock
        /// </summary>
        public void engageLock()
        {
            if (lockCoroutine != null)
                return;
            //set our coroutine ref and use the Lock Handler to start the coroutine
            lockCoroutine = lockOwner.StartCoroutine(lockProcess());
            //increment the lock handler's lock counter
            InteractableLockHandler.Instance.totalLocks++;
        }

        /// <summary>
        /// method for unlocking this lock.
        /// </summary>
        public void disengageLock()
        {
            if (lockCoroutine == null)
                return;

            // stop the locking coroutine
            lockOwner.StopCoroutine(lockCoroutine);

            //set the lock to unlocked(false)
            isLocked = false;
            //request that the lock handler updates the coresponding network interactable to be unlocked
            InteractableLockHandler.Instance.updateClientInteractableLocks(this, isLocked);
            //remove the lock from the lock handlers records
            deleteLock();
        }

        /// <summary>
        /// method for deleting this lock object
        /// </summary>
        private void deleteLock()
        {
            InteractableLockHandler.Instance.removeLock(this);
            InteractableLockHandler.Instance.totalLocks--;
        }
        
        /// <summary>
        /// Coroutine for handing the lock process. Essentially we are engaging the lock then
        /// waiting until the lock is unlocked or if the lock has been locked for the full
        /// LOCKOUT_DURATION to prevent interactables from being persistantly locked.
        /// </summary>
        private IEnumerator lockProcess()
        {
            //set the lock to be true
            isLocked = true;

            //request that the lock handler updates the coresponding network interactable to be locked
            InteractableLockHandler.Instance.updateClientInteractableLocks(this, isLocked);

            //halt this task for the LOCK_DURATION that is set in seconds
            yield return new WaitForSeconds(GlobalConstants.LOCK_DURATION);

            //assuming this lock hasnt be disengaged the following code will execute

            //set the lock to false
            isLocked = false;

            //request that the lock handler updates the coresponding network interactable to be unlocked
            InteractableLockHandler.Instance.updateClientInteractableLocks(this, isLocked);

            //remove the lock from the lock handlers records
            deleteLock();
        }

        public override bool Equals(object obj)
        {
            if(!(obj is InteractableLock))
                return false;

            return this.lockID.Equals(((InteractableLock)obj).lockID);
        }

        public override int GetHashCode()
        {
            return this.lockID.GetHashCode();
        }

    }
    #endregion InteractableLock Definiton

    /// <summary>
    /// The data structure for handling the active locks for each network interactable object tree.
    /// key: the parentKey of the network interactable. 
    /// Value: a set that stores an interactable lock for a network interactable that belongs to the parent key
    /// </summary>
    private Dictionary<int, HashSet<InteractableLock>> activeLocks { get; set; } = new Dictionary<int, HashSet<InteractableLock>>();

    /// <summary>
    /// The total amount of active locks
    /// </summary>
    public int totalLocks = 0;

    private void Awake()
    {
        ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
    }

    public void onStartupProcess()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            //if we're not the server then we leave
            if (!NetworkManager.Singleton.IsServer)
                return;

            registerMessages();

        };
        //Register lambda event that registers the messages of this manager on the most recent clients end
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
            ("interactableLockServerRequest", interactableLockServerRequest);

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler
            ("updateClientInteractableLocksClientBroadcast", updateClientInteractableLocksClientBroadcast);
    }

    /// <summary>
    /// method for removing a lock from the active locks dict
    /// </summary>
    /// <param name="interactableLock"></param>
    private void removeLock(InteractableLock interactableLock)
    {
        //remove the lock from our records
        activeLocks[interactableLock.lockID.parentKey].Remove(interactableLock);
    }

    /// <summary>
    /// method for sending a server request to start the locking process of an interactable over the network.
    /// </summary>
    /// <param name="interactable"></param>
    /// <param name="isLockoutRequest"></param>
    /// <returns></returns>
    public bool requestInteractableLock(MessageBasedInteractable interactable, bool isLockoutRequest)
    {
        //check if the interactable is already locked and if its not currently owned
        if (isLockoutRequest && interactable.isLocked())
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) requested that " +
                $"{interactable.name} should be locked but it is already");
            return false;
        }
        else if (!isLockoutRequest && !interactable.isLocked())
        {

            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) requested that " +
                $"{interactable.name} should be unlocked but it is already");
            return false;
        }
        if (interactable.isOwned())
        {
            DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) requested that " +
                $"{interactable.name} should be locked but client({interactable.getOwnerID()}) owns it");
            return false;
        }

        //we should be good to request the server to lockout this interactable

        //create the lockout request data
        InteractableLockRequest lockoutRequest = new InteractableLockRequest()
        {
            parentKey = interactable.lookupData.parentKey,
            objectIndex = interactable.lookupData.objectIndex,
            isLockoutRequest = isLockoutRequest
        };

        //create a new writer object to pack our message payload
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(lockoutRequest), Allocator.Temp);

        using (writer)
        {
            //pack our payload with the request data
            writer.WriteValueSafe(lockoutRequest);

            //send the request
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "interactableLockServerRequest", NetworkManager.ServerClientId, writer, NetworkDelivery.Reliable
                );
        }
        //we sent the message so we can return true
        return true;
    }

    /// <summary>
    /// message for requesting the server to set the lock state for the target interactable.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void interactableLockServerRequest(ulong senderID, FastBufferReader messagePayload)
    {
        InteractableLockRequest lockoutRequest;

        messagePayload.ReadValueSafe(out lockoutRequest);

        //try get the target network interactable
        MessageBasedInteractable targetInteractable = MessageBasedInstanceManager.Instance.
            lookupNetworkInteractable(lockoutRequest.parentKey, lockoutRequest.objectIndex);

        //check if the target interactable was found
        if (!targetInteractable)
        {
            DebugConsole.Instance.LogDebug($"Client({senderID}) requested that an object be locked" +
                $" but there was no object of key({lockoutRequest.parentKey}) index({lockoutRequest.objectIndex}).");
            return;
        }

        //check if the request is for locking the interactable
        if (lockoutRequest.isLockoutRequest)
        {
            //request the LockHandler to create a lock coresponding to the target interactable
            requestLockEngage(targetInteractable, senderID);
        }
        else// its for unlocking 
        {
            //request the LockHandler to create a lock coresponding to the target interactable
            requestLockDisengage(targetInteractable, senderID);
        }
    }

    /// <summary>
    /// method for requesting that an interactable be locked across the network. 
    /// </summary>
    /// <param name="networkInteractable"></param>
    /// <param name="clientID"></param>
    private void requestLockEngage(MessageBasedInteractable networkInteractable, ulong clientID)
    {
        //create a new lock ID object
        LockID lockID = new LockID()
        {
            parentKey = networkInteractable.lookupData.parentKey,
            objectIndex = networkInteractable.lookupData.objectIndex,
            clientID = clientID
        };

        //check if we have a entry for the parent key of the network interactable
        if (!activeLocks.ContainsKey(lockID.parentKey))
        {
            //initialse the entry
            activeLocks[lockID.parentKey] = new HashSet<InteractableLock>();
        }

        //create a new InteractableLock
        InteractableLock newLock = new InteractableLock(lockID,this);

        //check if we already have a lock for the coresponding network interactable
        if (activeLocks[lockID.parentKey].Contains(newLock))
        {
            //we already have a coresponding lock
            DebugConsole.Instance.LogDebug($"client({clientID}) requested that a lock be createed for {networkInteractable.name}" +
                $" but theres already an active lock.");
            return;
        }

        //add the new lock to the active lock dictionary
        activeLocks[lockID.parentKey].Add(newLock);
        //Engage the lock
        newLock.engageLock();
    }

    /// <summary>
    /// method for requesting the server to disengage the interactable lock for the target interactable
    /// across the network.
    /// </summary>
    /// <param name="networkInteractable"></param>
    /// <param name="clientID"></param>
    private void requestLockDisengage(MessageBasedInteractable networkInteractable, ulong clientID)
    {
        //create a new lock ID object
        LockID lockID = new LockID()
        {
            parentKey = networkInteractable.lookupData.parentKey,
            objectIndex = networkInteractable.lookupData.objectIndex,
            clientID = clientID
        };
        //create a proxy interactable lock to check if one exists in the active locks with the same values
        InteractableLock proxyLock = new InteractableLock(lockID, this);
        //declare a var for the target interactble Lock
        InteractableLock targetLock;
        //attempt to retrive targetlock
        activeLocks[lockID.parentKey].TryGetValue(proxyLock,out targetLock);

        //check if the targetlock exists
        if(targetLock == null)
        {
            DebugConsole.Instance.LogDebug($"client({clientID}) requested that the lock for {networkInteractable.name} to be " +
                $"disengaged but theres no such lock that is active.");
            return;
        }

        //it does so attempt to disengage the lock
        targetLock.disengageLock();
    }

    /// <summary>
    /// method for broadcasting a change in an interactable lock's lock state to all clients so they
    /// can update it that state on their end.
    /// </summary>
    /// <param name="interactableLock"></param>
    /// <param name="isLocked"></param>
    private void updateClientInteractableLocks(InteractableLock interactableLock, bool isLocked)
    {
        //check if the interactable lock exists 
        if(!activeLocks[interactableLock.lockID.parentKey].Contains(interactableLock))
        {
            DebugConsole.Instance.LogDebug($"A lock that client({interactableLock.lockID.clientID}) had requested stoped but" +
                $" for some reason wasnt in the active locks dict set");
            //it doesnt so return -- idk how thats possible but just incase of a freak event
            return;
        }
        //Create a lock request object to send to all clients -- as we are the server
        InteractableLockRequest lockRequest = new InteractableLockRequest
        {
            parentKey = interactableLock.lockID.parentKey,
            objectIndex = interactableLock.lockID.objectIndex,
            isLockoutRequest = isLocked
        };

        //create a writer to pack the message payload
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(lockRequest), Allocator.Temp);

        using (writer)
        {
            //pay the payload with our lock request
            writer.WriteValueSafe(lockRequest);
            //send the request to all clients 
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll
                (
                    "updateClientInteractableLocksClientBroadcast", writer,NetworkDelivery.Reliable
                );
        }
    }

    /// <summary>
    /// message for broadcasting a change in an interactable lock's locked state to all clients 
    /// so they can update the change locally on their end.
    /// </summary>
    /// <param name="senderID"></param>
    /// <param name="messagePayload"></param>
    private void updateClientInteractableLocksClientBroadcast(ulong senderID, FastBufferReader messagePayload)
    {
        InteractableLockRequest lockoutRequest;

        //unpack the request data
        messagePayload.ReadValueSafe(out lockoutRequest);

        //try to get the target interactable via the Instance Manager's Lookup table
        MessageBasedInteractable targetInteractable = MessageBasedInstanceManager.Instance.
            lookupNetworkInteractable(lockoutRequest.parentKey, lockoutRequest.objectIndex);
        //check if we found the target interactable in the lookup table
        if (!targetInteractable)
        {
            DebugConsole.Instance.LogError($"client({NetworkManager.Singleton.LocalClientId}) couldnt find the network interactable of " +
                $"parent key : ({lockoutRequest.parentKey}) object index : ({lockoutRequest.objectIndex})");
            return;
        }
        //update the lock state of the target interactable
        targetInteractable.toggleOwnershipLockout(lockoutRequest.isLockoutRequest);
    }

    /// <summary>
    /// method for requesting the server to lock multiple interactables that are involved in a collapse process
    /// then returns the list of interactables that need to be locked.
    /// </summary>
    /// <param name="interactable"></param>
    /// <param name="isSingleCollapse"></param>
    /// <returns></returns>
    public List<MessageBasedInteractable> requestCollapsingInteractableLockEngage(MessageBasedInteractable interactable, bool isSingleCollapse)
    {
        //check if this is a single collapse request
        if(isSingleCollapse)
        {
            //check if this is a lockout request and if any of the the collapsables are already locked
            if(interactable.areCollapsablesLockedSingle())
            {
                Debug.Log($"SINGLE: {interactable.name} has a relative that is already locked and this is a lock request.");
                return null;
            }
        }
        else
        {
            //check if this is a lockout request and if any of the the collapsables are already locked
            if (interactable.areCollapsablesLockedAll())
            {
                Debug.Log($"ALL: {interactable.name} has a relative that is already locked and this is a lock request.");
                return null;
            }
        }
        //get a list of all interacables involved in a collapse process
        List<MessageBasedInteractable> lockableInteractables = interactable.getCollapsableInteractables(isSingleCollapse);
        Debug.Log($"theres {lockableInteractables.Count} interactables in the list");

        //for each interactble that needs to be locked
        foreach(MessageBasedInteractable lockableInteractable in lockableInteractables)
        {
            //create a new collapse request object 
            InteractableLockRequest collapseLockRequest = new InteractableLockRequest
            {
                parentKey = lockableInteractable.lookupData.parentKey,
                objectIndex = lockableInteractable.lookupData.objectIndex,
                isLockoutRequest = true,
            };
            //create a new writer to pack the message payload
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(collapseLockRequest), Allocator.Temp);

            //send a request to the server to lock this interactable
            using (writer)
            {
                //pack the payload
                writer.WriteValueSafe(collapseLockRequest);

                //send server request
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                    (
                        "interactableLockServerRequest", NetworkManager.ServerClientId,writer,NetworkDelivery.Reliable
                    );
            }
        }

        return lockableInteractables;
    }

    /// <summary>
    /// method for requesting the server to unlock multiple interactables that are involved in a collapse process
    /// then returns the list of interactables that need to be unlocked.
    /// </summary>
    /// <param name="interactable"></param>
    /// <param name="isSingleCollapse"></param>
    /// <returns></returns>
    public void requestInteractableLockCollapseDisengage(MessageBasedInteractable interactable,bool isSingleCollapse)
    {
        //get a list of all the collapsable interactables
        List<MessageBasedInteractable> collapsableInteracables = interactable.getCollapsableInteractables(isSingleCollapse);


        //iterate over each interactable in the collapsable list 
        foreach(MessageBasedInteractable collapsableInteracable in collapsableInteracables)
        {
            Debug.Log($"{collapsableInteracable.name} should be collapsing");
            //check if the interactable is locked as we're trying to unlock it
            if(collapsableInteracable.isLocked())
            {
                //create request object
                InteractableLockRequest lockoutRequest = new InteractableLockRequest
                {
                    parentKey = collapsableInteracable.lookupData.parentKey,
                    objectIndex = collapsableInteracable.lookupData.objectIndex,
                    isLockoutRequest = false,
                };

                //create the writer to pack the payload
                var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(lockoutRequest), Allocator.Temp);

                using(writer)
                {
                    //pack the request data into the payload
                    writer.WriteValueSafe(lockoutRequest);

                    //send request to the server
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                        (
                            "interactableLockServerRequest", NetworkManager.ServerClientId, 
                            writer,NetworkDelivery.Reliable
                        );
                }
            }
        }

    }
}

/// <summary>
/// Data struct for indentifing an interactable lock
/// </summary>
public struct LockID
{
    /// <summary>
    /// the parent key for the interactable that is lock is for
    /// </summary>
    public int parentKey;
    /// <summary>
    /// /// the object index for the interactable that is lock is for
    /// </summary>
    public int objectIndex;
    /// <summary>
    /// the id of the client who made the request for the interactable to be locked
    /// </summary>
    public ulong clientID;

    public override bool Equals(object obj)
    {
        if (!(obj is LockID))
            return false;

        var other = (LockID)obj;
        return 
            this.parentKey == other.parentKey && 
            this.objectIndex == other.objectIndex && 
            this.clientID == other.clientID;
    }
    public override int GetHashCode()
    {
        return (parentKey, objectIndex, clientID).GetHashCode();
    }
}

/// <summary>
/// Serialised struct for holding request data for changing the lock state of an interactable
/// </summary>
public struct InteractableLockRequest : INetworkSerializable
{
    /// <summary>
    /// the parent key of the interactable
    /// </summary>
    public int parentKey;
    /// <summary>
    /// the object index of the interactable
    /// </summary>
    public int objectIndex;
    /// <summary>
    /// flag for marking if this request is to lock or unlock the interactable
    /// </summary>
    public bool isLockoutRequest;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);
        serializer.SerializeValue(ref isLockoutRequest);
    }
}


