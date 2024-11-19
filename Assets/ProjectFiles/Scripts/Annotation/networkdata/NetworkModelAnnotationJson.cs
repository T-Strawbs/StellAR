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
/// Network serialisable version of ModelAnnotationJson class able to be sent as parameters in Rpc calls
/// </summary>
public class NetworkModelAnnotationJson : INetworkSerializable
{
    /// <summary>
    /// The name of the model component that this object coresponds to.
    /// </summary>
    public string Name;
    /// <summary>
    /// the current highlight colour of the model component
    /// </summary>
    public string HighlightColour;
    /// <summary>
    /// the original colour of the model component
    /// </summary>
    public string OriginalColourCode;
    /// <summary>
    /// The
    /// </summary>
    public NetworkAnnotationJson[] Annotations;
    /// <summary>
    /// 
    /// </summary>
    public NetworkModelAnnotationJson[] Subcomponents;

    /// <summary>
    /// empty constructor to prevent null instantiations
    /// </summary>
    public NetworkModelAnnotationJson()
    {

    }

    /// <summary>
    /// convert input ModelAnnotationJson into NetworkModelAnnotationJson
    /// </summary>
    /// <param name="modelAnnotationJson"></param>
    public NetworkModelAnnotationJson(ModelAnnotationJson modelAnnotationJson)
    {
        Name = modelAnnotationJson.Name;
        HighlightColour = modelAnnotationJson.HighlightColour;

        if(modelAnnotationJson.OriginalColourCode == null)
        {
            OriginalColourCode = "NULL";
        }
        else
        {
            OriginalColourCode = modelAnnotationJson.OriginalColourCode;
        }

        // convert lists to arrays which can be serialised
        Annotations = new NetworkAnnotationJson[modelAnnotationJson.Annotations.Count];
        for (int i = 0; i < modelAnnotationJson.Annotations.Count; i++)
        {
            Annotations[i] = new NetworkAnnotationJson(modelAnnotationJson.Annotations[i]);
        }

        Subcomponents = new NetworkModelAnnotationJson[modelAnnotationJson.Subcomponents.Count];
        for (int i = 0; i < modelAnnotationJson.Subcomponents.Count; i++)
        {
            Subcomponents[i] = new NetworkModelAnnotationJson(modelAnnotationJson.Subcomponents[i]);
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref HighlightColour);
        serializer.SerializeValue(ref OriginalColourCode);
        serializer.SerializeValue(ref Annotations);
        serializer.SerializeValue(ref Subcomponents);
    }
}
