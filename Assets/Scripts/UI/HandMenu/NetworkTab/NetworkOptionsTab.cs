using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkOptionsTab : MonoBehaviour
{
   
    /// <summary>
    /// The button for displaying the connection options
    /// </summary>
    [SerializeField] private PressableButton networkTabBtn;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private OptionsTab optionsTab;
    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private JoinTab joinTab;
    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private DisconnectTab disconnectTab;

    [SerializeField] private OptionState optionsState = OptionState.OPTIONS;

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

    private void toggleNetworkTab()
    {
        DebugConsole.Instance.LogDebug($"the current tab is {currentTab.name} and is enabled: {currentTab.enabled}");

        currentTab.gameObject.SetActive(!currentTab.isActiveAndEnabled);
    }

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

public enum OptionState
{
    OPTIONS,
    JOINTAB,
    DISCONNECT,
}
