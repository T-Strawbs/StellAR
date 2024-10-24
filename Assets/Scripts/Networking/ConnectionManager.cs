using System;
using System.Collections;
using Unity;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionManager : Singleton<ConnectionManager>,PostStartupListener
{
    private void Awake()
    {
        ApplicationManager.Instance.onPostStartProcess.AddListener(onPostStartup);
    }

    public void onPostStartup()
    {
        startLocalConnection();
    }

    private void startLocalConnection()
    {
        //grab the network transport from the network manager
        UnityTransport localTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //set the address to local host so only this device can connect to itself
        localTransport.ConnectionData.Address = "127.0.0.1";
        //use the default port 7777
        localTransport.ConnectionData.Port = 7777;



        //start the server to run only locally
        NetworkManager.Singleton.StartHost();
    }
}