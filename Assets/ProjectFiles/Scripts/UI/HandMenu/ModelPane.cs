using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class that represents the UI menu for selecting a model to spawn from a list of ModelUI Elements.
/// </summary>
public class ModelPane : MonoBehaviour, ScrollablePane
{
    /// <summary>
    /// the button UI element for returning back to the home pane
    /// </summary>
    [SerializeField] private PressableButton backBtn;
    /// <summary>
    /// The tranform that will parent (hold) the UI elements
    /// </summary>
    [SerializeField] private RectTransform contentHolder;
    /// <summary>
    /// The prefab we instantiate to display and select the model
    /// we want to spawn
    /// </summary>
    [SerializeField] private ModelUI contentPrefab;
    /// <summary>
    /// the list for holding our ModelUI elements
    /// </summary>
    [SerializeField] private List<ModelUI> uiElements = new List<ModelUI>();
    /// <summary>
    /// the prefab for our default element we display when we have no
    /// models to select from.
    /// </summary>
    [SerializeField] private RectTransform defaultElement;

    private void Awake()
    {
        backBtn.OnClicked.AddListener(back);
    }

    /// <summary>
    /// method for closing the model pane and reopening the Home pane
    /// </summary>
    private void back()
    {
        HandMenuManager.Instance.activateHomePane();
    }

    public void populateScrollablePane(List<GameObject> loadedPrefabs)
    {   
        //check how many prefabs we have loaded, if we have none then 
        //activate the default UI element
        if (loadedPrefabs.Count < 1)
        {
            defaultElement.gameObject.SetActive(true);
            return;
        }
        //deactivate the default UI element
        defaultElement.gameObject.SetActive(false);
        //for each model that the PrefabManager has imported
        for (int i = 0; i < loadedPrefabs.Count; i++)
        {
            GameObject importedObject = loadedPrefabs[i];
            //instantiate animationName prefab
            ModelUI content = Instantiate<ModelUI>(contentPrefab);
            //set the parent and local transforms of animationName
            content.transform.SetParent(contentHolder);
            content.transform.localPosition = Vector3.zero;
            content.transform.localRotation = Quaternion.identity;
            content.transform.localScale = Vector3.one;
            //attempt to generate thumbnail
            Texture2D thumbnail = ThumbnailGenerator.Instance.getModelThumbnail(importedObject);
            //initialise animationName
            content.initialise(i, importedObject.name, thumbnail);
            uiElements.Add(content);
        }
    }


}
