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

    private List<Subscriber> subscribers = new List<Subscriber>();

    /// <summary>
    /// Sets the current selection.
    /// </summary>
    /// <param name="interactable"> the interactable</param>
    public void setSelection(Explodable interactable)
    {
        if(interactable == null)
        {
            Debug.Log("Cannot make selection as the iteractable is null");
            return;
        }    

        currentSelection = interactable;
        Debug.Log($"Current selection is {interactable.name}");
        //update subscribers
        foreach(Subscriber subscriber in subscribers)
        {
            subscriber.UpdateSubscriber(currentSelection);
        }
        //update UI
        UIManager.Instance.updateCurrentSelection(currentSelection.transform);
    }

    /// <summary>
    /// resets the current selection
    /// *might not be needed but is here for debugging*
    /// </summary>
    public void deactivateSelection() 
    {
        if(currentSelection == null) 
        {
            Debug.Log("Cannot make deselect as the current selection is already null");
            return;
        }
        Debug.Log($"Deselected {currentSelection.transform.gameObject.name}");
        currentSelection = null;
        
    }

    public void addSubscriber(Subscriber newSubscriber)
    {
        subscribers.Add(newSubscriber);
    }

    public Transform getCurrentSelectionParent()
    {
        if(!currentSelection)
        {
            Debug.Log($"there is not current selection so we cant get its parent");
            return null;
        }
        Explodable currentExplosive = currentSelection.GetComponent<Explodable>();
        if(!currentExplosive)
        {
            Debug.Log($"Something has gone terribly wrong cause our current selection " +
                $"({currentSelection.name}) doesnt have an Explodable script so we cant get its parent");
        }
        while(currentExplosive.getParent() != null) 
        { 
            currentExplosive = currentExplosive.getParent();
        }
        return currentExplosive.transform;

    }
    
}
