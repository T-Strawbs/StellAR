using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnnotationJson
{
    /// <summary>
    /// Component name
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// List of annotations
    /// </summary>
    public List<AnnotationJsonData> Annotations { get; set; }
    /// <summary>
    /// list of direct subcomponents
    /// </summary>
    public List<AnnotationJson> Subcomponents { get; set; }

    public AnnotationJson(String name)
    {
        Name = name;
        Annotations = new List<AnnotationJsonData>();
        Subcomponents = new List<AnnotationJson>();
    }
}
