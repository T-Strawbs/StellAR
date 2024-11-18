using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPane : MonoBehaviour
{
    [SerializeField] private PressableButton explodeBtn;
    [SerializeField] private PressableButton collapseSingleBtn;
    [SerializeField] private PressableButton collapseAllBtn;
    [SerializeField] private PressableButton activeHomePaneBtn;

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
        activeHomePaneBtn.OnClicked.AddListener(HandMenuManager.Instance.activeateHomePane);
    }
}
