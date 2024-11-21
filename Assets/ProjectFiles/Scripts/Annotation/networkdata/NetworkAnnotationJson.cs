using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Network serialisable version of AnnotationJson class able to be sent as parameters in Rpc calls
/// </summary>
public class NetworkAnnotationJson : INetworkSerializable
{
    /// <summary>
    /// The name of the model component that this annotation is for
    /// </summary>
    public string ComponentName;
    /// <summary>
    /// the name of the user who published this annotation
    /// </summary>
    public string Author;
    /// <summary>
    /// the timestamp of when this annotation is initalially published
    /// </summary>
    public string Timestamp;
    /// <summary>
    /// the format type of this annotation
    /// </summary>
    public string MessageType;
    /// <summary>
    /// The animationName of this annotation.
    /// </summary>
    public string Content;

    /// <summary>
    /// empty constructor to prevent null instantiations
    /// </summary>
    public NetworkAnnotationJson()
    {

    }

    /// <summary>
    /// convert input AnnotationJson into NetworkAnnotationJson
    /// </summary>
    /// <param name="annotation"></param>
    public NetworkAnnotationJson(AnnotationJson annotation)
    {
        ComponentName = annotation.ComponentName;
        Author = annotation.Author;
        Timestamp = annotation.Timestamp;
        MessageType = annotation.MessageType;
        Content = annotation.Content;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ComponentName);
        serializer.SerializeValue(ref Author);
        serializer.SerializeValue(ref Timestamp);
        serializer.SerializeValue(ref MessageType);
        serializer.SerializeValue(ref Content);
    }
}
