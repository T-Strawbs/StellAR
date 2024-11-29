using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class for managing the hand menu controls and various content panes.
/// </summary>
public class HandMenuManager : Singleton<HandMenuManager>, PrefabLoadListener
{
    /// <summary>
    /// the pane for activating the explosion/collapse process of the currently 
    /// selected interactable and activating the home pane
    /// </summary>
    [SerializeField] private ControlPane controlPane;
    /// <summary>
    /// the pane that displays a menu of the various features of our system
    /// </summary>
    [SerializeField] private RectTransform homePane;
    /// <summary>
    /// The pane for displaying a selection of spawnable model prefabs
    /// </summary>
    [SerializeField] private ModelPane modelPane;
    /// <summary>
    /// the pane for displaying all the animation clips for the currently
    /// selected model.
    /// </summary>
    [SerializeField] private AnimationPane animPane;
    /// <summary>
    /// the tab for establishing a connnection to a server by hosting or joining
    /// </summary>
    [SerializeField] private NetworkOptionsTab networkOptionsTab;

    private void Awake()
    {
        PrefabManager.Instance.OnPrefabsLoaded.AddListener(onPrefabsLoaded);

        //deactivate the model pane
        modelPane.enabled = false;
        //deactivate animations pane
        animPane.enabled = false;
        //deactivate the network options pane
        networkOptionsTab.enabled = false;

        //initalise the control pane
        controlPane.initialise();

        //intitalise the network opt tab
        networkOptionsTab.intialise();
    }

    /// <summary>
    /// method for activating the visibility of the model pane 
    /// whilst deactivating all other menu panes.
    /// </summary>
    public void activateModelPane()
    {
        modelPane.gameObject.SetActive(true);
        homePane.gameObject.SetActive(false);
        animPane.gameObject.SetActive(false);
    }
    /// <summary>
    /// method for activating the visibility of the animation pane 
    /// whilst deactivating all other menu panes.
    /// </summary>
    public void activateAnimationPane()
    {

        animPane.gameObject.SetActive(true);
        modelPane.gameObject.SetActive(false);
        homePane.gameObject.SetActive(false);
    }
    /// <summary>
    /// method for activating the visibility of the home pane 
    /// whilst deactivating all other menu panes.
    /// </summary>
    public void activateHomePane()
    {
        if(homePane.gameObject.activeInHierarchy)
        {
            homePane.gameObject.SetActive(false);
        }
        else
        {
            homePane.gameObject.SetActive(true);
        }
        modelPane.gameObject.SetActive(false);
        animPane.gameObject.SetActive(false);
    }
    /// <summary>
    /// method for closing all of the menu panes so that only
    /// the control pane remains active.
    /// </summary>
    public void resetHandMenu()
    {
        homePane.gameObject.SetActive(false);
        modelPane.gameObject.SetActive(false);
        animPane.gameObject.SetActive(false);
    }

    /// <param name="loadedPrefabs"></param>
    public void onPrefabsLoaded(List<GameObject> loadedPrefabs)
    {
        modelPane.populateScrollablePane(loadedPrefabs);
    }
}
