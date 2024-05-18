using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class AnnotationUI : MonoBehaviour
{
    [SerializeField] protected TMP_Text author;
    [SerializeField] protected TMP_Text timestamp;

    public virtual void initialise(AnnotationJsonData annotationData)
    {
        author.text = annotationData.Author;
        timestamp.text = annotationData.Timestamp;
    }
}
