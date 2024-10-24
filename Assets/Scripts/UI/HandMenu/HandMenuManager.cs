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


    private void Awake()
    {
        NetworkInteractablePrefabManager.Instance.OnImportCompleted.AddListener(populateScrollPanes);

        //deactivate it
        modelPane.enabled = false;
        //deactivate it
        animPane.enabled = false;
        //deactivate it

        //initalise the control pane
        controlPane.initialise();
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
