using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network serialisable version of ModelAnnotationJson class able to be sent as parameters in Rpc calls
/// </summary>
public class NetworkModelAnnotationJson : INetworkSerializable
{
    public string Name;
    public string HighlightColour;
    public string OriginalColourCode;
    public NetworkAnnotationJson[] Annotations;
    public NetworkModelAnnotationJson[] Subcomponents;

    // empty constructor to prevent null instantiations
    public NetworkModelAnnotationJson()
    {

    }

    // convert input ModelAnnotationJson into NetworkModelAnnotationJson
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
