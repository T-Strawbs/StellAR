using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class VoiceAnnotationJson : AnnotationJson
{
    public string AudioPath{ get; set; }
    public override string MessageType => GlobalConstants.VOICE_ANNOTATION;

    [JsonConstructorAttribute]
    public VoiceAnnotationJson(string componentName, string author, string timestamp, string content)
    {
        ComponentName = componentName;
        Author = author;
        Timestamp = timestamp;
        AudioPath = content;
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
    }
}
