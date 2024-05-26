using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class is a non monobehaviour singleton that manages
/// selections.
/// </summary>
public class SelectionManager : Singleton<SelectionManager>
{
    public static Explodable currentSelection;
    [SerializeField] private List<SelectionSubcriber> subscribers = new List<SelectionSubcriber>();

    /// <summary>
    /// Sets the current selection.
    /// </summary>
    /// <param name="interactable"> the interactable</param>
    public void setSelection(Explodable interactable)
    {
        if(interactable == null)
        {
            DebugConsole.Instance.LogError("Cannot make selection as the iteractable is null");
            return;
        }    

        currentSelection = interactable;
        DebugConsole.Instance.LogDebug($"Current selection is {interactable.name}");
        
        //update subscribers
        foreach(SelectionSubcriber subscriber in subscribers)
        {
            subscriber.updateSelection(currentSelection.transform);
        }
    }

    /// <summary>
    /// resets the current selection
    /// *might not be needed but is here for debugging*
    /// </summary>
    public void deactivateSelection() 
    {
        if(currentSelection == null) 
        {
            DebugConsole.Instance.LogError("Cannot make deselect as the current selection is already null");
            return;
        }
        DebugConsole.Instance.LogDebug($"Deselected {currentSelection.transform.gameObject.name}");
        currentSelection = null;
    }

    public void addSubscriber(SelectionSubcriber newSubscriber)
    {
        subscribers.Add(newSubscriber);
    }

    public Transform getCurrentSelectionParent()
    {
        if(!currentSelection)
        {
            DebugConsole.Instance.LogError($"there is not current selection so we cant get its parent");
            return null;
        }
        Explodable currentExplosive = currentSelection.GetComponent<Explodable>();
        if(!currentExplosive)
        {
            DebugConsole.Instance.LogError($"Something has gone terribly wrong cause our current selection " +
                $"({currentSelection.name}) doesnt have an Explodable script so we cant get its parent");
        }
        while(currentExplosive.getParent() != null) 
        { 
            currentExplosive = currentExplosive.getParent();
        }
        return currentExplosive.transform;

    }

    public void explodeSelection()
    {
        if (!currentSelection)
        {
            DebugConsole.Instance.LogError("Cannot explode as there is no current selection");
            return;
        }
        DebugConsole.Instance.LogDebug($"Exploding {currentSelection.name}.");
        currentSelection.explode();
    }

    public void collaspseSelection()
    {
        if (!currentSelection)
        {
            DebugConsole.Instance.LogError("Cannot collapse as there is no current selection");
            return;
        }
        DebugConsole.Instance.LogDebug($"collapsing {currentSelection.name} one level.");
        currentSelection.collapse();
    }

    public void collapseSelectionAll()
    {
        if (!currentSelection)
        {
            DebugConsole.Instance.LogError("Cannot collapse all as there is no current selection");
            return;
        }
        DebugConsole.Instance.LogDebug($"collapsing {currentSelection.name} on all levels");
        currentSelection.collapseAll();
    }
    
}
