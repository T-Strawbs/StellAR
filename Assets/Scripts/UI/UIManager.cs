using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TMP_Text currentSelectionText;
    [SerializeField] private MetadataPane metadataPane;
    [SerializeField] private AnnotationPane annotationPane;


    /// <summary>
    /// updates all relevant ui to the currently selected GO
    /// </summary>
    /// <param name="name"></param>
    public void updateCurrentSelection(Transform selection)
    {
        //grab the metadata component
        Metadata metadata = selection.GetComponent<Metadata>();
        if(!metadata)
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
        //update current selection text
        //currentSelectionText.text = $"Current Selection: {selection.name}";
    }

    private void updateMetadata(Metadata metadata)
    {
        metadataPane.updateMetadataContent(metadata.metadata);
    }

    private void updateAnnotations(AnnotationComponent annotationData)
    {
        //for each active UI element in the annotation pane
        foreach(AnnotationUI activeUI in annotationPane.ActiveAnnotationUI)
        {
            AnnotationUIGenerator.Instance.returnAnnotationUI(activeUI);
        }
        //clear the annotation pane's active UI list
        annotationPane.ActiveAnnotationUI.Clear();

        //populate the annotation pane's content
        //foreach annotation data from the selected GO's annotation component
        foreach(AnnotationJsonData annotationjsonData in annotationData.Annotations)
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
            //active the UI element
            annotationUI.gameObject.SetActive(true);
            //add to the annotation panes active elements
            annotationPane.ActiveAnnotationUI.Add(annotationUI);
        }
    }
}
