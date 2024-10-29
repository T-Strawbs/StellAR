using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JsonSubTypes;

[JsonConverter(typeof(JsonSubtypes), "MessageType")]
[JsonSubtypes.KnownSubType(typeof(TextAnnotationJson), "Text")]
[JsonSubtypes.KnownSubType(typeof(VoiceAnnotationJson), "Voice")]
[Serializable]
public abstract class AnnotationJson
{
    public string ComponentName { get; set; }
    public string Author { get; set; }
    public string Timestamp { get; set; }
    public abstract string MessageType { get; }
    public string Content { get; set; }

    public override bool Equals(object obj)
    {
        var other = obj as AnnotationJson;
        if (other == null)
            return false;
        return this.ComponentName == other.ComponentName &&
               this.Author == other.Author &&
               this.Timestamp == other.Timestamp;
    }
    public override int GetHashCode()
    {
        return (ComponentName, Author, Timestamp).GetHashCode();
    }
}
