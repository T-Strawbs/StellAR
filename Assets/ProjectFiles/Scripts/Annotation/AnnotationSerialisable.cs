using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// Please do not Remove
/// Orignal Authors:
///     � Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     � Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Serialised version of AnnotationList that also tracks a model's structure by storing an item's 
/// direct children (Subcomponents). Used for reading and writing annotations to disk.
/// </summary>
[Serializable]
public class AnnotationSerialisable
{
    /// <summary>
    /// Component name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Component highlight value
    /// </summary>
    public string HighlightColour { get; set; }

    /// <summary>
    /// Component highlight value
    /// </summary>
    public string OriginalColourCode { get; set; }

    /// <summary>
    /// List of annotations
    /// </summary>

    public List<AnnotationJson> Annotations { get; set; }
    /// <summary>
    /// list of direct subcomponents
    /// </summary>
    public List<AnnotationSerialisable> Subcomponents { get; set; }

    [JsonConstructorAttribute]
    public AnnotationSerialisable(String name)
    {
        Name = name;
        HighlightColour = "None";
        Annotations = new List<AnnotationJson>();
        Subcomponents = new List<AnnotationSerialisable>();
    }

    /// <summary>
    /// Convert Network Model Annotation into Model Annotation
    /// </summary>
    /// <param name="networkModelAnnotation">Network Model Annotation sent over network via Rpc</param>
    public AnnotationSerialisable(NetworkModelAnnotationJson networkModelAnnotation)
    {
        Name = networkModelAnnotation.Name;
        HighlightColour = networkModelAnnotation.HighlightColour;
        if(networkModelAnnotation.OriginalColourCode == "NULL")
        {
            OriginalColourCode = null;
        }
        else
        {
            OriginalColourCode = networkModelAnnotation.OriginalColourCode;
        }

        // Create empty list, loop over Network Annotations, convert them into AnnotationJson objects and add them to list
        Annotations = new List<AnnotationJson>();
        foreach(NetworkAnnotationJson networkAnnotation in networkModelAnnotation.Annotations)
        {

            // Convert network annotation into AnnotationJson
            AnnotationJson annotation = null;
            if(networkAnnotation.MessageType == GlobalConstants.TEXT_ANNOTATION)
            {
                annotation = new TextAnnotationJson(networkAnnotation);
            }
            else if(networkAnnotation.MessageType == GlobalConstants.VOICE_ANNOTATION)
            {
                annotation = new VoiceAnnotationJson(networkAnnotation);
            }
            else
            {
                DebugConsole.Instance.LogDebug("ERROR, tried to convert network annotation received via Rpc from server but it did not have a valid Message Type.");
            }
            
            // Add converted annotation to Annotations list
            if(annotation != null)
            {
                Annotations.Add(annotation);
            }
        }

        // Convert network subcomponents array
        Subcomponents = new List<AnnotationSerialisable>();
        foreach(NetworkModelAnnotationJson networkSubcomponent in networkModelAnnotation.Subcomponents)
        {
            AnnotationSerialisable subcomponent = new AnnotationSerialisable(networkSubcomponent);
            Subcomponents.Add(subcomponent);
        }
    }
}
