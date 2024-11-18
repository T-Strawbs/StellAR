using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class ModelAnnotationJson
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
    public List<ModelAnnotationJson> Subcomponents { get; set; }

    [JsonConstructorAttribute]
    public ModelAnnotationJson(String name)
    {
        Name = name;
        HighlightColour = "None";
        Annotations = new List<AnnotationJson>();
        Subcomponents = new List<ModelAnnotationJson>();
    }

    /// <summary>
    /// Convert Network Model Annotation into Model Annotation
    /// </summary>
    /// <param name="networkModelAnnotation">Network Model Annotation sent over network via Rpc</param>
    public ModelAnnotationJson(NetworkModelAnnotationJson networkModelAnnotation)
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
        Subcomponents = new List<ModelAnnotationJson>();
        foreach(NetworkModelAnnotationJson networkSubcomponent in networkModelAnnotation.Subcomponents)
        {
            ModelAnnotationJson subcomponent = new ModelAnnotationJson(networkSubcomponent);
            Subcomponents.Add(subcomponent);
        }
    }
}
