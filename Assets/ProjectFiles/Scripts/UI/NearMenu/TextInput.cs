using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class for facilitating the creation of text annotations via UI.
/// </summary>
public class TextInput : MonoBehaviour, IAnnotationInput
{
    /// <summary>
    /// the button that begins the annotation creation process
    /// </summary>
    [SerializeField] private PressableButton postBtn;
    /// <summary>
    /// the text field that we use as the annotation animationName
    /// </summary>
    [SerializeField] private TMP_Text inputText;
   
    private void Start()
    {
        //regisiter the post annoation callback to be invoked when the post button is activated
        postBtn.OnClicked.AddListener(postAnnotation);
    }

    public void postAnnotation ()
    {
        //check if the application is online
        if (ApplicationManager.Instance.isOnline())
        {
            postOnline();
        }
        else
        {
            postLocally();
        }
    }

    /// <summary>
    /// method for posting a new text annotation online
    /// </summary>
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
        AnnotationList annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationList>();
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
    /// <summary>
    /// method for posting a new text annotation locally
    /// </summary>
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
        AnnotationList annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationList>();
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
