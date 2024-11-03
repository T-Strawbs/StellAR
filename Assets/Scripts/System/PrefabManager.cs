using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PrefabManager : Singleton<PrefabManager>, StartupProcess
{
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private HashSet<int> spawnedPrefabs = new HashSet<int>();

    [NonSerialized] public UnityEvent<List<GameObject>> OnPrefabsLoaded = new UnityEvent<List<GameObject>>();

    [NonSerialized] public UnityEvent<GameObject> OnPrefabInstantiation = new UnityEvent<GameObject>();

    private void Awake()
    {
        ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
    }

    private void Start()
    {
        if (prefabs == null || prefabs.Count < 1)
            prefabs = new List<GameObject>();

        LoadPrefabs();
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

    public void registerMessages()
    {
        //intial request to spawn an interactable 
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("requestServerInteractableSpawn", requestServerInteractableSpawn);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("broadcastClientInteractbleSpawn", broadcastClientInteractbleSpawn);
    }

    private void LoadPrefabs()
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