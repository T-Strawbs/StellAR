using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnnotationComponent : MonoBehaviour
{
    public int annotationCount = 0;
    private List<AnnotationJson> annotations = new List<AnnotationJson>();
    public List<AnnotationJson> Annotations
    {
        get { return annotations; }
        set { annotations = value; }
    }


}
