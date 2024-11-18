using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class for managing the Data Panel's content.
/// </summary>
public class DataPanelManager : Singleton<DataPanelManager>, NewSelectionListener
{
    /// <summary>
    /// The Object representing the Metadata pane
    /// </summary>
    [SerializeField] private MetadataPane metadataPane;
    /// <summary>
    /// The Object representing the Annotation pane
    /// </summary>
    [SerializeField] private AnnotationPane annotationPane;

    private void Awake()
    {
        //subscribe the selection manager's onLocalSelectionChanged event
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(onNewSelection);
    }

    public void onNewSelection(Transform selection)
    {
        //grab the metadata component
        MetadataComponent metadata = selection.GetComponent<MetadataComponent>();
        if (!metadata)
        {
            Debug.Log($"Cant grab metadata from {selection.name}");
            return;
        }
        AnnotationComponent annotationData = selection.GetComponent<AnnotationComponent>();
        if (!annotationData)
        {
            Debug.Log($"Cant grab annotation data from {selection.name}");
            return;
        }
        //populate metadata UI
        updateMetadata(metadata);
        //populate annotation UI
        updateAnnotations(annotationData);
    }

    /// <summary>
    /// method for updating the metadata pane with content from the MetadataComponent of the
    /// currently selected interactable
    /// </summary>
    /// <param name="metadata"></param>
    private void updateMetadata(MetadataComponent metadata)
    {
        if(string.IsNullOrEmpty(metadata.metadata))
        {
            metadataPane.updateMetadataContent($"No Available Metadata for {metadata.name}");
            return;
        }
        metadataPane.updateMetadataContent(metadata.metadata);
    }
    /// <summary>
    /// method for updating the metadata pane with content from the AnnotationComponent of the
    /// currently selected interactable
    /// </summary>
    /// <param name="annotationData"></param>
    public void updateAnnotations(AnnotationComponent annotationData)
    {
        Debug.Log("We are updating annotations");
        //for each active UI element in the annotation pane
        for (int i = 0; i < annotationPane.ActiveAnnotationUI.Count; i++)
        {
            //grab a reference to the annotation UI object from the active annotation UI pool.
            AnnotationUI activeUI = annotationPane.ActiveAnnotationUI[i];
            Debug.Log($"Returning {activeUI.transform.name}");
            //return element to the generator for cleansing
            AnnotationUIGenerator.Instance.returnAnnotationUI(activeUI);
        }

        Debug.Log("Clearing the annotation panes active content");
        //clear the annotation pane's active UI list
        annotationPane.ActiveAnnotationUI.Clear();

        //check if the annotations list is null
        if (annotationData.Annotations == null)
        {
            DebugConsole.Instance.LogDebug("the annotations of this component is null");
            return;
        }
        //for each annotation data from the current interactble's annotations
        foreach (AnnotationJson annotationjsonData in annotationData.Annotations)
        {
            //get a Annotation UI element from the AnnotationGenerator
            AnnotationUI annotationUI = AnnotationUIGenerator.Instance.GetAnnotationUI(annotationjsonData);
            if(!annotationUI)
            {
                Debug.LogError($"annotation UI couldnt be genenerated");
                continue;
            }
            //set the parent to the annotation pane's content holder
            annotationUI.transform.SetParent(annotationPane.ContentHolder);
            //reset the annotation UI's local transforoms
            annotationUI.transform.localPosition = Vector3.zero;
            annotationUI.transform.localRotation = Quaternion.identity;
            annotationUI.transform.localScale = Vector3.one;
            //activate the UI element
            annotationUI.gameObject.SetActive(true);
            //add to the annotation panes active elements
            annotationPane.ActiveAnnotationUI.Add(annotationUI);
        }
    }


}
