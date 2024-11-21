using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class that handles the highlighting of interactables by changing the currently selected 
/// interactable's colour when a colour button is pressed. 
/// 
/// Updates the annotation manager when an interactable's highlight is changed so it can 
/// update the model's annotation json file.
/// </summary>
public class HighlightManager : NetworkSingleton<HighlightManager>
{
    /// <summary>
    /// Button for clearing the current highlight of an interactable 
    /// so that it returns to it's original colour
    /// </summary>
    [SerializeField]
    private PressableButton noneButton;
    /// <summary>
    /// Button for changing the current highlight of an interactable 
    /// to red
    /// </summary>
    [SerializeField]
    private PressableButton redButton;
    /// <summary>
    /// Button for changing the current highlight of an interactable 
    /// to green.
    /// </summary>
    [SerializeField]
    private PressableButton greenButton;
    /// <summary>
    /// Button for changing the current highlight of an interactable 
    /// to blue
    /// </summary>
    [SerializeField]
    private PressableButton blueButton;
    /// <summary>
    /// Button for changing the current highlight of an interactable 
    /// to yellow
    /// </summary>
    [SerializeField]
    private PressableButton yellowButton;

    private void Awake()
    {
        noneButton.OnClicked.AddListener(() => setHighlight("None"));
        redButton.OnClicked.AddListener(() => setHighlight("Red"));
        greenButton.OnClicked.AddListener(() => setHighlight("Green"));
        blueButton.OnClicked.AddListener(() => setHighlight("Blue"));
        yellowButton.OnClicked.AddListener(() => setHighlight("Yellow"));

    }

    /// <summary>
    /// method for setting the colour of the highlight applied to the currently
    /// selected interactable using a colour code passed through as a string.
    /// </summary>
    /// <param name="colour"></param>
    private void setHighlight(string colour)
    {
        if (!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError("Cannot set highlight as there is no current selection");
            return;
        }

        //if user is offline
        if(!ApplicationManager.Instance.isOnline())
        {
            // change the colour
            DebugConsole.Instance.LogDebug($"setting highlight {SelectionManager.Instance.currentSelection.name} to {colour}");
            SelectionManager.Instance.currentSelection.GetComponent<AnnotationList>().changeHighlightColour(colour);

            // update the Json
            AnnotationManager.Instance.updateAnnotationHighlightJson(SelectionManager.Instance.currentSelection.transform, colour);
        }
        //if user is online
        else
        {
            DebugConsole.Instance.LogDebug($"setting highlight {SelectionManager.Instance.currentSelection.name} to {colour}");
            MessageBasedInteractable colourThisObject = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();

            if(colourThisObject == null)
            {
                DebugConsole.Instance.LogError("Tried to set highlight while online but current selection is not networked (MessageBasedInteractable).");
            }
            else
            {
                // send string of colour to highlight and lookup data for object to highlight to the server
                setHighlightServerRpc(colourThisObject.lookupData, colour);
            }
        }
    }

    /// <summary>
    /// RPC for requesting that a highlight of the currrently selected interactable 
    /// is syncronised across the network and is save to the server's disk.
    /// </summary>
    /// <param name="lookupData"></param>
    /// <param name="colour"></param>
    [Rpc(SendTo.Server)]
    private void setHighlightServerRpc(NetworkInteractableLookupData lookupData, string colour)
    {
        //change colour of object on all devices
        setHighlightClientRpc(lookupData, colour);

        //update JSON on disk to contain new colour
        AnnotationManager.Instance.updateAnnotationHighlightJson(MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).transform, colour);
    }
    /// <summary>
    /// RPC for updating the highlight of a given interactable locally on the clients end.
    /// </summary>
    /// <param name="lookupData"></param>
    /// <param name="colour"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void setHighlightClientRpc(NetworkInteractableLookupData lookupData, string colour)
    {
        //change colour of object
        MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).GetComponent<AnnotationList>().changeHighlightColour(colour);
    }
}