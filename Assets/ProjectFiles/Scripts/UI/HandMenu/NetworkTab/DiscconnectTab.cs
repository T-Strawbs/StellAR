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
/// Class representing the Disconnect Tab UI element that allows users to disconnect from the
/// server they are currently connected to.
/// </summary>
public class DisconnectTab : NetworkTab
{
    /// <summary>
    /// The disconnect button of the disconnection tab
    /// </summary>
    [SerializeField] private PressableButton disconnectBtn;

    public override void initialise(NetworkOptionsTab networkTab)
    {
        //ensure that this networktab is deactived
        gameObject.SetActive(false);

        disconnectBtn.OnClicked.AddListener(disconnect);

        this.networkTab = networkTab;
    }

    /// <summary>
    /// Disconnects from the server and sets the network tab back to the network options
    /// </summary>
    private void disconnect()
    {
        ConnectionManager.Instance.disconnect();

        networkTab.setOptionState(OptionState.OPTIONS);
    }
}