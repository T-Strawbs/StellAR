using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuManager : MonoBehaviour
{
    [SerializeField] private RectTransform homePane;
    [SerializeField] private ModelPane modelPane;
    [SerializeField] private AnimationPane animPane;

    private void start()
    {
        initialisePanes();
    }

    private void initialisePanes()
    {
        //populate model pane
        modelPane.populateScrollPane();
        //deactivate it
        modelPane.enabled = false;
        //populate anim pane
        animPane.populateScrollPane();
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
        homePane.gameObject.SetActive(true);
        modelPane.gameObject.SetActive(false);
        animPane.gameObject.SetActive(false);
    }


}
