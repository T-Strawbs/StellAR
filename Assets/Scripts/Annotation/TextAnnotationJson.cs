using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TextAnnotationJson : AnnotationJson
{
    public override string MessageType => GlobalConstants.TEXT_ANNOTATION;

    [JsonConstructorAttribute]
    public TextAnnotationJson(string componentName, string author, string timestamp, string content)
    {
        ComponentName = componentName;
        Author = author;
        Timestamp = timestamp;
        Content = content;
    }

    /// <summary>
    /// Convert network annotation to Text Annotation
    /// </summary>
    /// <param name="networkAnnotation">Network annotation passed over network via Rpc</param>
    public TextAnnotationJson(NetworkAnnotationJson networkAnnotation)
    {
        ComponentName = networkAnnotation.ComponentName;
        Author = networkAnnotation.Author;
        Timestamp = networkAnnotation.Timestamp;
        Content = networkAnnotation.Content;
    }
}
