using MixedReality.Toolkit.UX;
using System;
using Unity;
using UnityEngine;

public class OptionsTab : NetworkTab
{
    private NetworkOptionsTab networkTab;
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

    private void host()
    {
        if(!ConnectionManager.Instance.startHost())
        {
            return;
        }

        //update the network tab to change to the disconnect tab
        networkTab.setOptionState(OptionState.DISCONNECT);
    }

    private void joinTab()
    {
        //update the network tab to show the join tab
        networkTab.setOptionState(OptionState.JOINTAB);
    }
}