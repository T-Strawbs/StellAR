using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkInteractablePrefabManager : Singleton<NetworkInteractablePrefabManager>
{
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private HashSet<int> spawnedPrefabs = new HashSet<int>();

    public UnityEvent<List<GameObject>> OnImportCompleted = new UnityEvent<List<GameObject>>();

    private void Start()
    {
        if (prefabs == null || prefabs.Count < 1)
            prefabs = new List<GameObject>();

        LoadPrefabs();

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

    private void LoadPrefabs()
    {
        //get all the prefab paths from the prefab dir
        string[] prefabPaths = Directory.GetFiles(GlobalConstants.PREFAB_DIR, "*.prefab");

        if (prefabPaths.Length < 1)
        {
            DebugConsole.Instance.LogDebug($"couldnt load prefabs from {GlobalConstants.PREFAB_DIR}");
            return;
        }

        //for each file path
        foreach (string prefabFilePath in prefabPaths)
        {
            //make the path relative so that its Prefabs/PrefabName
            string prefabPath = $"Prefabs/{Path.GetFileNameWithoutExtension(prefabFilePath)}";
            //load the gameobject
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            //check if the prefab was loaded
            if (!prefab)
            {
                Debug.Log($"We couldnt load {prefabPath} from {prefabFilePath}");
                continue;
            }
            //add it to the prefabs list
            prefabs.Add(prefab);
        }

        OnImportCompleted.Invoke(prefabs);
    }

    private void registerMessages()
    {
        #region MessageRegistering
        //intial request to spawn an interactable 
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("requestServerInteractableSpawn", requestServerInteractableSpawn);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("broadcastClientInteractbleSpawn", broadcastClientInteractbleSpawn);
        #endregion MessageRegistering
    }
    /// <summary>
    /// sender method for requesting the server to spawn an interactable.
    /// </summary>
    /// <param name="prefabIndex">the index of the prefab in the prefab list</param>
    public void requestInteractbleSpawn(int prefabIndex,Vector3 spawnPosition)
    {
        //check if the index is within bounds 
        if (prefabIndex < 0 || prefabIndex >= prefabs.Count)
            return;

        //create a new spawn request object
        SpawnRequest spawnRequest = new SpawnRequest { prefabIndex = prefabIndex,spawnPosition = spawnPosition};

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
                    "requestServerInteractableSpawn", NetworkManager.ServerClientId ,writer, NetworkDelivery.Reliable
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

        //instantiate object
        GameObject instance = Instantiate(prefabs[spawnRequest.prefabIndex], spawnRequest.spawnPosition, Quaternion.identity);
        //add instance to the Interactable Instance Manager list
        NetworkInteractableInstanceManager.Instance.registerNetworkInteractable(instance.GetComponent<NetworkInteractable>());
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
    public Vector3 spawnPosition;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref prefabIndex);
        serializer.SerializeValue(ref spawnPosition);
    }
}
