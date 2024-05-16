using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text currentSelectionText;
    [SerializeField] private GameObject metadataUI;
    [SerializeField] private GameObject annotationUI;

    /// <summary>
    /// updates all relevant ui to the currently selected GO
    /// </summary>
    /// <param name="name"></param>
    public void updateCurrentSelection(Transform selection)
    {
        //populate metadata UI
        updateMetadata();
        //populate annotation UI
        updateAnnotations();
        //update current selection text
        //currentSelectionText.text = $"Current Selection: {selection.name}";
    }

    private void updateMetadata()
    {
       
    }

    private void updateAnnotations()
    {

    }
}
