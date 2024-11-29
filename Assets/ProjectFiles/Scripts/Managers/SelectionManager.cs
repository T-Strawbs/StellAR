using Microsoft.MixedReality.GraphicsTools;
using System;
using System.Collections;   
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class for managing the current selection of interactables and updating the systems that need to know when a 
/// new selection has been made.
/// </summary>
public class SelectionManager : Singleton<SelectionManager>
{
    /// <summary>
    /// The current selection for local actions like updating the near menu and such
    /// </summary>
    public Interactable currentSelection {  get; private set; }
    /// <summary>
    /// The subscribable event thats invoked when the current local selection is changed.
    /// Subscribers should derrive from the NewSelectionListener interface.
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

    /// <summary>
    /// callback method for starting the explosion process of interactables
    /// </summary>
    public void explodeCurrentInteractable()
    {
        //check if we have a current selection
        if (!currentSelection)
            return;
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is about to request an explosion of {currentSelection.name}");

        // if current selection has a mesh outline remove it before exploding
        ClientManager.Instance.removeOutline(currentSelection.transform);

        //check if the current selection is a MessageBasedInteractable (Networked Interactable)
        if (currentSelection is MessageBasedInteractable)
            //request the server to explode the current selection
            MessageBasedExplodableHandler.Instance.requestInteractableExplostion((MessageBasedInteractable)currentSelection);
        //check if the current selection is a LocalBasedInteractble
        else if (currentSelection is LocalBasedInteractable)
            //tell the current selection to explode
            currentSelection.explodeInteractable();

    }

    /// <summary>
    /// callback method for starting the collapse process of interactables for one level.
    /// </summary>
    public void collapseCurrentInteractableSingle()
    {
        //check if we have a current selection
        if (!currentSelection)
            return;
        //we're collapsing the current selection on one level
        bool isSingleCollapse = true;
        
        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is about to request the single collapse of {currentSelection.name}");

        //check if the current selection is a MessageBasedInteractable (Networked Interactable)
        if (currentSelection is MessageBasedInteractable)
            //request the server to collapse the current selection on one level
            MessageBasedExplodableHandler.Instance.requestInteractableCollapse((MessageBasedInteractable)currentSelection, isSingleCollapse);
        //check if the current selection is a LocalBasedInteractble
        else if (currentSelection is LocalBasedInteractable)
            //tell the current selection to collapse on one level
            currentSelection.collapseInteractable(isSingleCollapse);
    }

    /// <summary>
    /// callback method for starting the collapse process of interactables for the whole model
    /// </summary>
    public void collapseCurrentInteractableAll()
    {
        //check if we have a current selection
        if (!currentSelection)
            return;
        //we're collapsing the current selection completely
        bool isSingleCollapse = false;

        DebugConsole.Instance.LogDebug($"client({NetworkManager.Singleton.LocalClientId}) is about to request the complete collapse of {currentSelection.name}");

        //check if the current selection is a MessageBasedInteractable (Networked Interactable)
        if (currentSelection is MessageBasedInteractable)
            //request the server to collapse the current selection completely
            MessageBasedExplodableHandler.Instance.requestInteractableCollapse((MessageBasedInteractable)currentSelection, isSingleCollapse);
        //check if the current selection is a LocalBasedInteractble
        else if (currentSelection is LocalBasedInteractable)
            //tell the current selection to collapse completely
            currentSelection.collapseInteractable(isSingleCollapse);
    }

    /// <summary>
    /// method for getting the root parent (the model's root object) of a given component of a model
    /// </summary>
    /// <param name="componentTransform"></param>
    /// <returns></returns>
    public Transform getModelRoot(Transform componentTransform) 
    {
        //Grab the interactable component from the componentTransform
        Interactable currentInteractable = componentTransform.GetComponent<Interactable>();
        //check if theres a interactable component
        if(!currentInteractable)
        {
            DebugConsole.Instance.LogError($"SM_getModelRoot: tried to get root of a component that is not an Interactable");
            return null;
        }
        //recursively climb up model hierachy to find the model root
        while(currentInteractable.parent != null)
        {
            currentInteractable = currentInteractable.parent;
        }
        //return the root object of the model
        return currentInteractable.transform;
    }
}
