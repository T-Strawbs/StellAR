using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class TextInput : MonoBehaviour, IAnnotationInput
{
    [SerializeField] private PressableButton inputFieldBtn;
    [SerializeField] private PressableButton postBtn;
    [SerializeField] private TMP_Text inputText;
   
    private void Start()
    {

        postBtn.OnClicked.AddListener(postAnnotation);

    }

    public void postAnnotation ()
    {
        Debug.Log($"POSTING ANNOTATION!");
        //check if theres a currently selected object
        if (!SelectionManager.currentSelection)
        {
            Debug.Log("We cant post when we have no currently selected object");
            return;
        }
        //quickly assert that the current selection has an annotation component
        AnnotationComponent annotationComponent = SelectionManager.currentSelection.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            Debug.Log("We cant post as the currently selected object has no annotation component");
            return;
        }
        //get the current date and time
        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        //tell Annotation manager to create annotation Json
        AnnotationManager.Instance.createAnnotationJson(
            SelectionManager.currentSelection.name,
            "Text",
            "Default Author",// we need to replace this once we have multiple active users
            currentDateTime,
            inputText.text
            );
        //tell the UI manager to update its annotations 
        UIManager.Instance.updateAnnotations(annotationComponent);
        Debug.Log("we wouldve \"created\" a text annotation");
    }

    private void invokeKeyboard()
    {
        Debug.Log("we would invoke the keyboard here");
    }


}