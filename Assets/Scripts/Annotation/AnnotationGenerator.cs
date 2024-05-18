using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class AnnotationGenerator: MonoBehaviour
{
    [SerializeField] private List<AnnotationUI> annotationPrefabs; 

    public AnnotationUI generateAnnotationUI(AnnotationJsonData annotationData)
    {
        if(annotationData.MessageType == "Text")
        {
            if (annotationPrefabs[0] is not TextAnnotationUI)
            {
                Debug.LogError($"Annotation[0] is type of {annotationPrefabs[0].GetType()} not TextAnnotationUI");
            }
            TextAnnotationUI annotationUI = (TextAnnotationUI) Instantiate<AnnotationUI>(annotationPrefabs[0]);
            //map the annotation data to the UI elements
            annotationUI.initialise(annotationData);
            //return the 
            return annotationUI;
        }
        else if(annotationData.MessageType == "voice")
        {
            AnnotationUI annotationUI = Instantiate<AnnotationUI>(annotationPrefabs[1]);
        }
        return null;
    }
}
