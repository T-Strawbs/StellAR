using System;
using UnityEngine;
using Unity.Netcode;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Serialised Struct for holding data that the system use for looking up MessageBasedInteractables
/// using the MessageBasedInstanceManager's lookup table.
/// </summary>
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