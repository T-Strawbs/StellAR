using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class representing the UI for the NetworkOptionsTab. Allows the user to host a server, join a server
/// via ip entered into the inputfield or disconnecting from an active server.
/// </summary>
public class NetworkOptionsTab : MonoBehaviour
{
    /// <summary>
    /// The button for displaying the connection options
    /// </summary>
    [SerializeField] private PressableButton networkTabBtn;
    /// <summary>
    /// the UI element that displays the controls for hosting/joining a server
    /// </summary>
    [SerializeField] private OptionsTab optionsTab;
    /// <summary>
    /// The UI element for inputing the ip address of the desired server and 
    /// a button for joining.
    /// </summary>
    [SerializeField] private JoinTab joinTab;
    /// <summary>
    /// the UI element with a button for disconnecting from the active server.
    /// </summary>
    [SerializeField] private DisconnectTab disconnectTab;
    /// <summary>
    /// Enum for tracking the current state of the NetworkOptionsTab
    /// </summary>
    [SerializeField] private OptionState optionsState = OptionState.OPTIONS;
    /// <summary>
    /// Reference to the currently active tab UI element
    /// </summary>
    [SerializeField] private NetworkTab currentTab;

    /// <summary>
    /// initialises the Network options tabs ui elements
    /// </summary>
    public void intialise()
    {
        //initialise the tabs
        optionsTab.initialise(this);
        joinTab.initialise(this);
        disconnectTab.initialise(this);

        currentTab = optionsTab;

        networkTabBtn.OnClicked.AddListener(toggleNetworkTab);
    }

    /// <summary>
    /// callback method for activating and deactivating the NetworkOptionsTab
    /// </summary>
    private void toggleNetworkTab()
    {
        DebugConsole.Instance.LogDebug($"the current tab is {currentTab.name} and is enabled: {currentTab.enabled}");

        currentTab.gameObject.SetActive(!currentTab.isActiveAndEnabled);
    }

    /// <summary>
    /// method for toggling the state of the NetworkOptionsTab to 
    /// change the current options tab.
    /// </summary>
    /// <param name="newState"></param>
    public void setOptionState(OptionState newState)
    {
        //deactivate the current tab
        currentTab.gameObject.SetActive(false);
        //set the current option state
        optionsState = newState;
        //set the current tab to match the current state
        switch(optionsState)
        {
            case OptionState.OPTIONS:
                //set the current tab to the option tab
                currentTab = optionsTab;
                break;
            case OptionState.DISCONNECT:
                //set the current tab to the disconnect tab
                currentTab = disconnectTab;
                break;
            case OptionState.JOINTAB:
                //set the current tab to the join tab
                currentTab = joinTab;
                break;  
        }
        //activate the current tab
        currentTab.gameObject.SetActive(true);
    }
        
}

/// <summary>
/// Enum for controlling the current state of the NetworkOptionsTab
/// </summary>
public enum OptionState
{
    OPTIONS,
    JOINTAB,
    DISCONNECT,
}
