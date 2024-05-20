using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnnotationComponent : MonoBehaviour
{
    public int annotationCount = 0;

    public List<AnnotationJson> Annotations
    {
        get;
        set;
    }

    private void Start()
    {
        Annotations = new List<AnnotationJson>();
    }

    private void FixedUpdate()
    {
        annotationCount = Annotations.Count;
    }

}
