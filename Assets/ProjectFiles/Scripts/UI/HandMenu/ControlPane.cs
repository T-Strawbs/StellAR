using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class represeting the control pane that pops up when the hand menu is activated
/// </summary>
public class ControlPane : MonoBehaviour
{
    /// <summary>
    /// The button UI element for exploding the currently selected interactable
    /// </summary>
    [SerializeField] private PressableButton explodeBtn;
    /// <summary>
    /// The button UI element for collapsing the currently selected interactable on one level
    /// </summary>
    [SerializeField] private PressableButton collapseSingleBtn;
    /// <summary>
    /// The button UI element for collapsing the currently selected interactable on all levels
    /// </summary>
    [SerializeField] private PressableButton collapseAllBtn;
    /// <summary>
    /// The button UI element for activating the home menu
    /// </summary>
    [SerializeField] private PressableButton activeHomePaneBtn;

    /// <summary>
    /// method for initialising this pane
    /// </summary>
    public void initialise()
    {
        //initalise the on click event for the explode btn to tell the selectionManager to explode the current selection
        explodeBtn.OnClicked.AddListener(SelectionManager.Instance.explodeCurrentInteractable);
        //initalise the on click event for the collapse single btn to tell the selectionManager to colapse the current selection
        //one level
        collapseSingleBtn.OnClicked.AddListener(SelectionManager.Instance.collapseCurrentInteractableSingle);
        //initalise the on click event for the collapse single btn to tell the selectionManager to colapse the current selection
        //all levels to root.
        collapseAllBtn.OnClicked.AddListener(SelectionManager.Instance.collapseCurrentInteractableAll);
        //initalise the on click event for the home btn to activate/deactivate the home pane of the handmenu
        activeHomePaneBtn.OnClicked.AddListener(HandMenuManager.Instance.activateHomePane);
    }
}
