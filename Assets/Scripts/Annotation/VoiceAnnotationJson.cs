using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class VoiceAnnotationJson : AnnotationJson
{
    public string AudioPath{ get; set; }
    public override string MessageType => "Voice";
}
