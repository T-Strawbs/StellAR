using MixedReality.Toolkit.UX;
using System;
using Unity;
using UnityEngine;

public class JoinTab : NetworkTab
{
    private NetworkOptionsTab networkTab;
    /// <summary>
    /// The back button of the join tab
    /// </summary>
    [SerializeField] private PressableButton backBtn;
    /// <summary>
    /// The join button
    /// </summary>
    [SerializeField] private PressableButton joinBtn;
    /// <summary>
    /// The input field for entering the ip
    /// </summary>
    [SerializeField] private MRTKTMPInputField ipInputField;

    public override void initialise(NetworkOptionsTab networkTab)
    {
        //ensure that this networktab is deactived
        gameObject.SetActive(false);

        this.networkTab = networkTab;

        backBtn.OnClicked.AddListener(back);

        joinBtn.OnClicked.AddListener(join);
    }

    /// <summary>
    /// joins the client to the server with the address of the ip field.
    /// </summary>
    private void join()
    {
        //grab the input field ip
        string ipAddress = ipInputField.text;
        if(string.IsNullOrEmpty(ipAddress))
        {
            //we should let the user know that the input field is empty
            DebugConsole.Instance.LogDebug($"wont be able to join server as the ip field is empty");
            return;
        }
        //join the server with the ip address
        if(!ConnectionManager.Instance.joinServer(ipAddress))
        {
            DebugConsole.Instance.LogDebug($"JT_JOIN() CONNECTION MANAGER FAILED TO JOIN");
            //we weren't sucessfull so just return
            return;
        }
        //update the networktabs current state to show the disconnect tab
        networkTab.setOptionState(OptionState.DISCONNECT);

    }

    /// <summary>
    /// returns the networktab back to options
    /// </summary>
    private void back()
    {
        //set the network tabs current state back to options
        networkTab.setOptionState(OptionState.OPTIONS);
    }
}