using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>,SelectionSubcriber
{
    [SerializeField] private MetadataPane metadataPane;
    [SerializeField] private AnnotationPane annotationPane;

    public void Awake()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(updateSelection);
    }
    private void updateMetadata(MetadataComponent metadata)
    {
        if(string.IsNullOrEmpty(metadata.metadata))
        {
            metadataPane.updateMetadataContent($"No Available Metadata for {metadata.name}");
            return;
        }
        metadataPane.updateMetadataContent(metadata.metadata);
    }

    public void updateAnnotations(AnnotationComponent annotationData)
    {
        Debug.Log("We are updating annotations");
        //for each active UI element in the annotation pane
        for (int i = 0; i < annotationPane.ActiveAnnotationUI.Count; i++)
        {
            AnnotationUI activeUI = annotationPane.ActiveAnnotationUI[i];
            Debug.Log($"Returning {activeUI.transform.name}");
            //return element to the generator
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

    public void updateSelection(Transform selection)
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
}
