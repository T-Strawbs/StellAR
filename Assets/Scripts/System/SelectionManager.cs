using System;
using System.Collections;   
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : Singleton<SelectionManager>
{
    /// <summary>
    /// The current selection for local actions like updating the near menu and such
    /// </summary>
    public Interactable currentSelection {  get; private set; }
   /// <summary>
   /// The subscribable event thats invoked when the current local selection is changed
   /// </summary>
    [NonSerialized] public UnityEvent<Transform> onLocalSelectionChanged = new UnityEvent<Transform>();
    /// <summary>
    /// sets the current local selection to the newly selected interactable
    /// </summary>
    /// <param name="newSelection">The newly selected interactable</param>
    public void setCurrentSelection(Interactable newSelection)
    {
        //update the current selection
        currentSelection = newSelection;
        //invoke the event to notify all subscribers
        onLocalSelectionChanged.Invoke(currentSelection.transform);
    }

    public void explodeCurrentInteractable()
    {
        if (!currentSelection)
            return;
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is about to request an explosion of {currentSelection.name}");

        if(currentSelection is MessageBasedInteractable)
            MessageBasedExplodableHandler.Instance.requestInteractableExplostion((MessageBasedInteractable)currentSelection);
        else if(currentSelection is LocalBasedInteractble)
            currentSelection.explodeInteractable();

    }

    public void collapseCurrentInteractableSingle()
    {
        if (!currentSelection)
            return;

        bool isSingleCollapse = true;
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is about to request the single collapse of {currentSelection.name}");
        if (currentSelection is MessageBasedInteractable)
            MessageBasedExplodableHandler.Instance.requestInteractableCollapse((MessageBasedInteractable)currentSelection, isSingleCollapse);
        else if (currentSelection is LocalBasedInteractble)
            currentSelection.collapseInteractable(isSingleCollapse);
    }

    public void collapseCurrentInteractableAll()
    {
        if (!currentSelection)
            return;

        bool isSingleCollapse = false;
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is about to request the complete collapse of {currentSelection.name}");
        if (currentSelection is MessageBasedInteractable)
            MessageBasedExplodableHandler.Instance.requestInteractableCollapse((MessageBasedInteractable)currentSelection, isSingleCollapse);
        else if (currentSelection is LocalBasedInteractble)
            currentSelection.collapseInteractable(isSingleCollapse);
    }

    public Transform getModelRoot(Transform component) 
    {
        Interactable currentInteractable = component.GetComponent<Interactable>();
        if(!currentInteractable)
        {
            DebugConsole.Instance.LogError($"SM_getModelRoot: tried to get root of a component that is not an Interactable");
            return null;
        }
            
        while(currentInteractable.parent != null)
        {
            currentInteractable = currentInteractable.parent;
        }

        return currentInteractable.transform;
    }
}
