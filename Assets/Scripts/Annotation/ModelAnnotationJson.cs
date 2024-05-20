using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModelAnnotationJson
{
    /// <summary>
    /// Component name
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// List of annotations
    /// </summary>
    public List<AnnotationJson> Annotations { get; set; }
    /// <summary>
    /// list of direct subcomponents
    /// </summary>
    public List<ModelAnnotationJson> Subcomponents { get; set; }

    public ModelAnnotationJson(String name)
    {
        Name = name;
        Annotations = new List<AnnotationJson>();
        Subcomponents = new List<ModelAnnotationJson>();
    }
}
