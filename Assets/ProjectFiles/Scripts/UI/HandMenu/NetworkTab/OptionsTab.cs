using MixedReality.Toolkit.UX;
using System;
using Unity;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class representing the options tab for hosting or joining a server.
/// </summary>
public class OptionsTab : NetworkTab
{
    /// <summary>
    /// The button for hosting 
    /// </summary>
    [SerializeField] private PressableButton hostBtn;
    /// <summary>
    /// the button for enabling/disabling the join tab
    /// </summary>
    [SerializeField] private PressableButton joinTabBtn;

    public override void initialise(NetworkOptionsTab networkTab)
    {
        //ensure that this networktab is deactived
        gameObject.SetActive(false);

        //set the host btn to invoke the start host method of the ConnectionManager
        hostBtn.OnClicked.AddListener(host);
        //set the join btn to tell the network tab to show the join tab
        joinTabBtn.OnClicked.AddListener(joinTab);

        this.networkTab = networkTab;
    }

    /// <summary>
    /// method for starting the hosting process throught the connection manager
    /// </summary>
    private void host()
    {
        if(!ConnectionManager.Instance.startHost())
        {
            return;
        }

        //update the network tab to change to the disconnect tab
        networkTab.setOptionState(OptionState.DISCONNECT);
    }
    /// <summary>
    /// method for activating the join tab via the NetworkOptionsTab
    /// </summary>
    private void joinTab()
    {
        //update the network tab to show the join tab
        networkTab.setOptionState(OptionState.JOINTAB);
    }
}