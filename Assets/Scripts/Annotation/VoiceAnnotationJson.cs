using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class VoiceAnnotationJson : AnnotationJson
{
    public override string MessageType => GlobalConstants.VOICE_ANNOTATION;

    /// <summary>
    /// Create a VoiceAnnotationJson, content is the file path to the audio file
    /// </summary>
    /// <param name="componentName"></param>
    /// <param name="author"></param>
    /// <param name="timestamp"></param>
    /// <param name="content"></param>
    [JsonConstructorAttribute]
    public VoiceAnnotationJson(string componentName, string author, string timestamp, string content)
    {
        ComponentName = componentName;
        Author = author;
        Timestamp = timestamp;
        base.Content = content;
    }

    /// <summary>
    /// Convert network annotation to Voice Annotation
    /// </summary>
    /// <param name="networkAnnotation">Network annotation passed over network via Rpc</param>
    public VoiceAnnotationJson(NetworkAnnotationJson networkAnnotation)
    {
        ComponentName = networkAnnotation.ComponentName;
        Author = networkAnnotation.Author;
        Timestamp = networkAnnotation.Timestamp;
        base.Content = networkAnnotation.Content;
    }
}
