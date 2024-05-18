using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnnotationComponent : MonoBehaviour
{
    public int annotationCount = 0;

    public List<AnnotationJsonData> Annotations
    {
        get;
        set;
    }

    private void Start()
    {
        Annotations = new List<AnnotationJsonData>();
    }

    private void FixedUpdate()
    {
        annotationCount = Annotations.Count;
    }

}
