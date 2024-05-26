using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuManager : MonoBehaviour
{
    [SerializeField] private RectTransform homePane;
    [SerializeField] private ModelPane modelPane;
    [SerializeField] private AnimationPane animPane;

    private void Start()
    {
        initialisePanes();
    }

    private void initialisePanes()
    {
        //populate model pane
        modelPane.populateScrollPane();
        //deactivate it
        modelPane.enabled = false;
        //deactivate it
        animPane.enabled = false;
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


}
