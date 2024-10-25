using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandMenuManager : Singleton<HandMenuManager>, ImportListener
{
    [SerializeField] private RectTransform homePane;
    [SerializeField] private ModelPane modelPane;
    [SerializeField] private AnimationPane animPane;
    [SerializeField] private ControlPane controlPane;

    [SerializeField] private NetworkOptionsTab networkOptionsTab;


    private void Awake()
    {
        PrefabManager.Instance.OnImportCompleted.AddListener(populateScrollPanes);

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

    private void populateScrollPanes(List<GameObject> importeObjects)
    {
        modelPane.populateScrollPane(importeObjects);
    }

    public void activateModelPane()
    {
        modelPane.gameObject.SetActive(true);
        homePane.gameObject.SetActive(false);
        animPane.gameObject.SetActive(false);
    }

    public void activateAnimationPane()
    {

        animPane.gameObject.SetActive(true);
        modelPane.gameObject.SetActive(false);
        homePane.gameObject.SetActive(false);
    }

    public void activeateHomePane()
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

    public void resetHandMenu()
    {
        homePane.gameObject.SetActive(false);
        modelPane.gameObject.SetActive(false);
        animPane.gameObject.SetActive(false);
    }

    public void OnImportComplete(List<GameObject> gameObjects)
    {
        throw new System.NotImplementedException();
    }
}
