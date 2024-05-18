using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class AnnotationJsonData
{
    public string Author { get; set; }
    public string Timestamp { get; set; }
    public abstract string MessageType { get; }
}
