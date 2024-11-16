using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MixedReality.Toolkit.UX;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Abstract class for Annotation UI elements 
/// </summary>
public abstract class AnnotationUI : MonoBehaviour
{
    /// <summary>
    /// the display field for the author of the annotation data 
    /// </summary>
    [SerializeField] protected TMP_Text author;
    /// <summary>
    /// the display field for the timestamp of the annotation data 
    /// </summary>
    [SerializeField] protected TMP_Text timestamp;
    /// <summary>
    /// pressable ui button for deleting the annotation
    /// </summary>
    [SerializeField] protected PressableButton deleteBtn;
    /// <summary>
    /// the annotation data object for this annotation element
    /// </summary>
    private AnnotationJson annotationData;

    private void Awake()
    {
        //add the delete annotation method as a callback that is invoked when 
        //the delete btn is clicked
        deleteBtn.OnClicked.AddListener(deleteAnnotaton);
    }

    /// <summary>
    /// method for initialising the annoation UI element with new annotation data
    /// </summary>
    /// <param name="annotationData"></param>
    public virtual void initialise(AnnotationJson annotationData)
    {
        author.text = annotationData.Author;
        timestamp.text = annotationData.Timestamp;
        this.annotationData = annotationData;
    }
    /// <summary>
    /// method for deleteing this annotation from the selected interactables's annoation data
    /// </summary>
    private void deleteAnnotaton()
    {
        //check if the application is online
        if (ApplicationManager.Instance.isOnline())
        {
            //get a reference to the currently selected message based interactable
            MessageBasedInteractable currentSelection = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();
            if (currentSelection == null)//check if its real
            {
                DebugConsole.Instance.LogError("Attempted to delete annotation while online from non-networked object (not a MessageBasedInteractable)");
            }
            else
            {
                //Package a new Network Annotation Json so we can delete it on the hosts end. 
                NetworkAnnotationJson annotationToDelete = new NetworkAnnotationJson(annotationData);
                //Request the annotation manager to delete this annotation on the hosts end.
                AnnotationManager.Instance.deleteAnnotationServerRpc(currentSelection.lookupData, annotationToDelete);
            }
        }
        else//we're offline so delete this annotation locally.
        {
            AnnotationManager.Instance.deleteAnnotationFromDisk(annotationData, SelectionManager.Instance.currentSelection.transform);
        }
    }
}
