using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TextAnnotationJsonData : AnnotationJsonData
{
    public string Content { get; set; }
    public override string MessageType => "Text";
}
