using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnnotationUI : AnnotationUI
{
    [SerializeField] private TMP_Text content;

    public override void initialise(AnnotationJson annotationData)
    {
        base.initialise(annotationData);
        if(annotationData is TextAnnotationJson textMessage)
        {
            content.text = textMessage.Content;
        }
        else
        {
            DebugConsole.Instance.LogError($"Invalid message type parsed to {transform.name}");
        }
    }
}
