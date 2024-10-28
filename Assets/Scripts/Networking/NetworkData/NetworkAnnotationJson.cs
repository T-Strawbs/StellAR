using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network serialisable version of AnnotationJson class able to be sent as parameters in Rpc calls
/// </summary>
public class NetworkAnnotationJson : INetworkSerializable
{
    public string ComponentName;
    public string Author;
    public string Timestamp;
    public string MessageType;

    // empty constructor to prevent null instantiations

    public NetworkAnnotationJson()
    {

    }

    // convert input AnnotationJson into NetworkAnnotationJson
    public NetworkAnnotationJson(AnnotationJson annotation)
    {
        ComponentName = annotation.ComponentName;
        Author = annotation.Author;
        Timestamp = annotation.Timestamp;
        MessageType = annotation.MessageType;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ComponentName);
        serializer.SerializeValue(ref Author);
        serializer.SerializeValue(ref Timestamp);
        serializer.SerializeValue(ref MessageType);
    }
}
