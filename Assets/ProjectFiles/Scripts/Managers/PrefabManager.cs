using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class that handles the spawning of interactables both local and across the network.
/// </summary>
public class PrefabManager : Singleton<PrefabManager>, StartupProcess
{
    /// <summary>
    /// The list of prefabs that can be spawned in the scene.
    /// </summary>
    [SerializeField] private List<GameObject> prefabs;
    /// <summary>
    /// a list of prefabs that have already been spawned. Uses the prefab's index position from the prefab list as a hash value.
    /// </summary>
    [SerializeField] private HashSet<int> spawnedPrefabs = new HashSet<int>();
    /// <summary>
    /// Unity event for invoking behaviour when the system's prefabs have finished loading in.
    /// </summary>
    [NonSerialized] public UnityEvent<List<GameObject>> OnPrefabsLoaded = new UnityEvent<List<GameObject>>();
    /// <summary>
    /// Unity Event that triggers when a prefab has been instantiated.
    /// </summary>
    [NonSerialized] public UnityEvent<GameObject> OnPrefabInstantiation = new UnityEvent<GameObject>();


    private void Awake()
    {
        ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);


    }

    private void Start()
    {
        if (prefabs == null || prefabs.Count < 1)
            prefabs = new List<GameObject>();

        loadPrefabs();
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
    /// method for registering the prefab manager's custom network messages 
    /// </summary>
    private void registerMessages()
    {
        //intial request to spawn an interactable 
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("requestServerInteractableSpawn", requestServerInteractableSpawn);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("broadcastClientInteractbleSpawn", broadcastClientInteractbleSpawn);
    }

    /// <summary>
    /// Method for loading the user's prefabs from the build's PREFAB_DIR
    /// </summary>
    private void loadPrefabs()
    {
        //get all the prefab paths from the prefab dir
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs");

        if (loadedPrefabs.Length < 1)
        {
            DebugConsole.Instance.LogDebug($"couldnt load prefabs from {GlobalConstants.PREFAB_DIR}");
            return;
        }

        //for each file path
        foreach (GameObject prefab in loadedPrefabs)
        {
            //check if the prefab was loaded
            if (prefab)
            {
                //add it to the prefabs list
                prefabs.Add(prefab);
            }
            else
            {
                DebugConsole.Instance.LogDebug("PFM_loadPrefabs(): Had an issue loading in a prefab");
            }
        }
        DebugConsole.Instance.LogDebug("PFM_loadPrefabs(): finished loading prefabs");
        OnPrefabsLoaded.Invoke(prefabs);
    }

    /// <summary>
    /// sender method for requesting the server to spawn an interactable.
    /// </summary>
    /// <param name="prefabIndex">the index of the prefab in the prefab list</param>
    public void requestInteractbleSpawn(int prefabIndex)
    {
        //check if the index is within bounds 
        if (prefabIndex < 0 || prefabIndex >= prefabs.Count)
            return;

        if (spawnedPrefabs.Contains(prefabIndex))
        {
            DebugConsole.Instance.LogWarning
                (
                    $"Tried to request the spawning of {prefabs[prefabIndex].name} but we already have a copy"
                );
            return;
        }

        //check if we are in offline mode or in Online Mode
        if(ApplicationManager.Instance.isOnline())
        {
            //its online 
            networkRequestInteractableSpawn(ref prefabIndex);
            return;
        }

        //spawn locally
        localRequestInteractableSpawn(prefabIndex);
    }

    /// <summary>
    /// method for spawning in a given prefab via prefab index as a local interactable.
    /// </summary>
    /// <param name="prefabIndex"></param>
    private void localRequestInteractableSpawn(int prefabIndex)
    {
        //instantiate object -- need camera view transform or vuforia target
        GameObject instance = Instantiate
            (
                prefabs[prefabIndex], 
                ApplicationManager.Instance.calculateIntantiationTransform(), 
                Quaternion.identity
            );

        //rename the instance to match the prefab data
        instance.name = prefabs[prefabIndex].name;
        
        if(!instance)
        {
            DebugConsole.Instance.LogDebug
                ($"PFM_localRequestInteractableSpawn(): Cannot spawn prefab of index: {prefabIndex}");
            return;
        }
        //prepare the local interactable
        InteractableFactory.Instance.initialiseInteractable(InteractableType.LocalBased,instance);

        //add the prefab index to the spawned prefabs list
        spawnedPrefabs.Add(prefabIndex);

        //tell all PrefabInstantiationListeners that we instantiated a prefab
        OnPrefabInstantiation.Invoke(instance);
    }

    /// <summary>
    /// Method for spawning in a given prefab via prefab index as a messagebased interactable over the network.
    /// </summary>
    /// <param name="prefabIndex"></param>
    private void networkRequestInteractableSpawn(ref int prefabIndex)
    {
        //create a new spawn request object
        SpawnRequest spawnRequest = new SpawnRequest { prefabIndex = prefabIndex };

        //create a new writer that will pack a message payload with the byte size of the spawn request
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(spawnRequest), Allocator.Temp);

        using (writer)
        {
            DebugConsole.Instance.LogDebug("Requesting the spawning of an object.");

            //pack the spawn request into a message payload
            writer.WriteValueSafe(spawnRequest);
            //send request to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "requestServerInteractableSpawn", NetworkManager.ServerClientId, writer, NetworkDelivery.Reliable
                );
        }
    }
    /// <summary>
    /// Message for requesting the server to spawn a given prefab across the network.
    /// </summary>
    /// <param name="senderId"></param>
    /// <param name="messagePayload"></param>
    private void requestServerInteractableSpawn(ulong senderId, FastBufferReader messagePayload)
    {
        DebugConsole.Instance.LogDebug($"client({senderId}) told client/server({NetworkManager.Singleton.LocalClientId}) to spawn an object");
        SpawnRequest spawnRequest;

        //read the data from the payload and output it
        messagePayload.ReadValueSafe(out spawnRequest);

        //check if the object has been spawned yet
        if (spawnedPrefabs.Contains(spawnRequest.prefabIndex))
        {
            DebugConsole.Instance.LogDebug($"client({senderId}) wanted to spawn a " +
                $"{prefabs[spawnRequest.prefabIndex]?.name} but we have already spawned one");
            //the object was already spawned
            return;
        }
        
        //create a new writer object for us to pack our message payload
        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(spawnRequest), Allocator.Temp);
        
        using (writer)
        {
            DebugConsole.Instance.LogDebug("Requesting the spawning of an object.");
            //write the spawn request to the writer
            writer.WriteValueSafe(spawnRequest);
            //send request to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll
                (
                    "broadcastClientInteractbleSpawn", writer, NetworkDelivery.Reliable
                );
        }
    }

    /// <summary>
    /// message for broadcasting to clients to spawn a given prefab as a messagebased interactable locally on their end.
    /// </summary>
    /// <param name="senderId"></param>
    /// <param name="messagePayload"></param>
    private void broadcastClientInteractbleSpawn(ulong senderId, FastBufferReader messagePayload)
    {
        SpawnRequest spawnRequest;

        //read the data from the payload and output it
        messagePayload.ReadValueSafe(out spawnRequest);

        DebugConsole.Instance.LogDebug($"client:{senderId} requested that an object should be spawned on client:{NetworkManager.Singleton.LocalClientId}'s device");

        Transform networkOriginTransform = VuforiaManager.networkOriginObject.transform;

        //instantiate object -- nededs to be using vufoira target
        GameObject instance = Instantiate(prefabs[spawnRequest.prefabIndex]);

        instance.transform.SetParent(networkOriginTransform, false);
        instance.transform.SetPositionAndRotation(networkOriginTransform.position, networkOriginTransform.rotation);

        //rename the instance to match the prefab data
        instance.name = prefabs[spawnRequest.prefabIndex].name;

        //prepare the messagge based interactable
        InteractableFactory.Instance.initialiseInteractable(InteractableType.MessageBased,instance);

        //add instance to the MessageBasedInteractable Instance Manager list
        MessageBasedInstanceManager.Instance.registerNetworkInteractable(instance.GetComponent<MessageBasedInteractable>());

        //tell all PrefabInstantiationListeners that we instantiated a prefab
        OnPrefabInstantiation.Invoke(instance);

        //attempt to register the prefab as already spawned
        try 
        {
            spawnedPrefabs.Add(spawnRequest.prefabIndex);
        }
        catch(Exception e)
        {
            DebugConsole.Instance.LogError($"Client({NetworkManager.Singleton.LocalClientId})__Caught::{e.ToString()}");
        }
    }
}

public struct SpawnRequest : INetworkSerializable
{
    public int prefabIndex;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref prefabIndex);
    }
}
