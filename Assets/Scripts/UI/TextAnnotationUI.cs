using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnnotationUI : AnnotationUI
{
    [SerializeField] private TMP_Text content;

    public override void initialise(AnnotationJsonData annotationData)
    {
        base.initialise(annotationData);
        if(annotationData is TextAnnotationJsonData textMessage)
        {
            content.text = textMessage.Content;
        }
        else
        {
            Debug.LogError($"Invalid message type parsed to {transform.name}");
        }
    }
}
