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

    [NonSerialized] public UnityEvent onStartupProcess = new UnityEvent();

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
        onStartupProcess.Invoke();
    }

    public void setNetworkStatus(NetworkStatus networkStatus)
    {
        this.networkStatus = networkStatus; 
    }

    public bool isOnline()
    {
        return networkStatus == NetworkStatus.ONLINE;
    }


    /// <summary>
    /// Calculates the target position to instantiate the prefab so that the object is in front of the user
    /// </summary>
    /// <returns></returns>
    public Vector3 calculateIntantiationTransform()
    {
        //the target distance of the position the object should spawn at
        float distanceFromCamera = 2f;
        //calculate the target position to spawn the object in direction of the player camera at the set distance.
        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera;
        return targetPosition;
    }

}
public enum NetworkStatus { ONLINE,OFFLINE}