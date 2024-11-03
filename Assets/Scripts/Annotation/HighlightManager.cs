using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HighlightManager : NetworkSingleton<HighlightManager>
{
    [SerializeField]
    private PressableButton noneButton;
    [SerializeField]
    private PressableButton redButton;
    [SerializeField]
    private PressableButton greenButton;
    [SerializeField]
    private PressableButton blueButton;
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
            SelectionManager.Instance.currentSelection.GetComponent<AnnotationComponent>().changeHighlightColour(colour);

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

    [Rpc(SendTo.Server)]
    private void setHighlightServerRpc(NetworkInteractableLookupData lookupData, string colour)
    {
        //change colour of object on all devices
        setHighlightClientRpc(lookupData, colour);

        //update JSON on disk to contain new colour
        AnnotationManager.Instance.updateAnnotationHighlightJson(MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).transform, colour);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void setHighlightClientRpc(NetworkInteractableLookupData lookupData, string colour)
    {
        //change colour of object
        MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).GetComponent<AnnotationComponent>().changeHighlightColour(colour);
    }
}