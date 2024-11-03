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
        if (ApplicationManager.Instance.isOnline())
        {
            postOnline();
        }
        else
        {
            postLocally();
        }
    }

    private void postOnline()
    {
        DebugConsole.Instance.LogDebug($"POSTING ANNOTATION ONLINE!");

        //check if currently selected object exists and is networked (MessageBasedInteractable)
        MessageBasedInteractable currentSelection = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();
        if (!currentSelection)
        {
            DebugConsole.Instance.LogError("We cant post when we have no currently selected object that is MessageBasedInteractable");
            return;
        }
        if (string.IsNullOrEmpty(inputText.text))
        {
            DebugConsole.Instance.LogError("Cannot post as input field is empty or null");
            return;
        }
        //quickly assert that the current selection has an annotation component
        AnnotationComponent annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("We cant post as the currently selected object has no annotation component");
            return;
        }

        // post annotation to server
        AnnotationManager.Instance.postAnnotationServerRpc(currentSelection.lookupData, inputText.text, GlobalConstants.TEXT_ANNOTATION);

        //reset text input field
        inputText.text = "";
    }
    private void postLocally()
    {
        DebugConsole.Instance.LogDebug($"POSTING ANNOTATION!");
        //check if theres a currently selected object
        if (!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError("We cant post when we have no currently selected object");
            return;
        }
        if (string.IsNullOrEmpty(inputText.text))
        {
            DebugConsole.Instance.LogError("Cannot post as input field is empty or null");
            return;
        }
        //quickly assert that the current selection has an annotation component
        AnnotationComponent annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("We cant post as the currently selected object has no annotation component");
            return;
        }
        //get the current date and time
        string currentDateTime = DateTime.Now.ToString(GlobalConstants.TIME_FORMAT);
        //tell Annotation manager to create annotation Json
        AnnotationManager.Instance.createAnnotationJson(
            SelectionManager.Instance.currentSelection.transform,
            GlobalConstants.TEXT_ANNOTATION,
            "Default Author",// we need to replace this once we have multiple active users
            currentDateTime,
            inputText.text
            );
        //tell the UI manager to update its annotations 
        DataPanelManager.Instance.updateAnnotations(annotationComponent);
        DebugConsole.Instance.LogDebug("we wouldve \"created\" a text annotation");
        //reset text input field
        inputText.text = "";
    }

}
