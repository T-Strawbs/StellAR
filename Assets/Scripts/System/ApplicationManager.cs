using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// class that manages the 
/// </summary>
public class ApplicationManager : Singleton<ApplicationManager>
{

    [NonSerialized] public UnityEvent onProcessCustomMessengers = new UnityEvent();

    [NonSerialized] public UnityEvent onPostStartProcess = new UnityEvent();

    public NetworkStatus networkStatus { get; private set; }

    private void Awake()
    {
        networkStatus = NetworkStatus.OFFLINE;
    }
    private void Start()
    {
        startupProcess();
    }

    private void startupProcess()
    {
        //setup custom message handlers
        onProcessCustomMessengers.Invoke();
    }

    public void setNetworkStatus(NetworkStatus networkStatus)
    {
        this.networkStatus = networkStatus; 
    }

    public bool isOnline()
    {
        return networkStatus == NetworkStatus.ONLINE;
    }
}
public enum NetworkStatus { ONLINE,OFFLINE}