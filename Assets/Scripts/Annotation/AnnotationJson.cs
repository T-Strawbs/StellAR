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
}
