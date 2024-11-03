using MixedReality.Toolkit.UX;
using System;
using Unity;
using UnityEngine;

public class DisconnectTab : NetworkTab
{

    private NetworkOptionsTab networkTab;

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