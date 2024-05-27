using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MixedReality.Toolkit.UX;

public abstract class AnnotationUI : MonoBehaviour
{
    [SerializeField] protected TMP_Text author;
    [SerializeField] protected TMP_Text timestamp;
    [SerializeField] protected PressableButton deleteBtn;
    private AnnotationJson annotationData;
    private void Awake()
    {
        deleteBtn.OnClicked.AddListener(deleteAnnotaton);
    }
    public virtual void initialise(AnnotationJson annotationData)
    {
        author.text = annotationData.Author;
        timestamp.text = annotationData.Timestamp;
        this.annotationData = annotationData;
    }

    private void deleteAnnotaton()
    {
        AnnotationManager.Instance.deleteAnnotation(annotationData);
    }
}
