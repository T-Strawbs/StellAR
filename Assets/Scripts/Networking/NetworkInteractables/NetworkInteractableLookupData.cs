using System;
using UnityEngine;
using Unity.Netcode;

public struct NetworkInteractableLookupData : INetworkSerializable
{
    /// <summary>
    /// The name of the root object for this interactable
    /// </summary>
    public int parentKey;
    /// <summary>
    /// The name of this interactable
    /// </summary>
    public int objectIndex;

    // Enables lookup data struct to be sent as parameters in Rpc functions
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref parentKey);
        serializer.SerializeValue(ref objectIndex);
    }
}