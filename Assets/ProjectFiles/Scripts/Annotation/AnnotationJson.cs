using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JsonSubTypes;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Serialised abstract class for holding Json objects that are Annotation Messages.
/// </summary>
[JsonConverter(typeof(JsonSubtypes), "MessageType")]
[JsonSubtypes.KnownSubType(typeof(TextAnnotationJson), "Text")]
[JsonSubtypes.KnownSubType(typeof(VoiceAnnotationJson), "Voice")]
[Serializable]
public abstract class AnnotationJson
{
    /// <summary>
    /// The name of the model component that this annotation is for
    /// </summary>
    public string ComponentName { get; set; }
    /// <summary>
    /// the name of the user who published this annotation
    /// </summary>
    public string Author { get; set; }
    /// <summary>
    /// the timestamp of when this annotation is initalially published
    /// </summary>
    public string Timestamp { get; set; }
    /// <summary>
    /// the format type of this annotation
    /// </summary>
    public abstract string MessageType { get; }
    /// <summary>
    /// The content of this annotation.
    /// </summary>
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
