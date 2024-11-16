using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// class that Manages the application's systems, ensuring that they are initialised in specific 
/// sequences and behaviour of the network status of the app
/// </summary>
public class ApplicationManager : Singleton<ApplicationManager>
{
    /// <summary>
    /// Subscribable event for processes that need to occur on the startup phase of the application
    /// </summary>
    [NonSerialized] public UnityEvent onStartupProcess = new UnityEvent();
    /// <summary>
    /// Subscribable event for processes that need to occur after the startup phase of the application
    /// </summary>
    [NonSerialized] public UnityEvent onPostStartupProcess = new UnityEvent();
    /// <summary>
    /// Enum for recognising the network status of the application ONLINE or OFFLINE
    /// </summary>
    public NetworkStatus networkStatus { get; private set; }

    private void Awake()
    {
        networkStatus = NetworkStatus.OFFLINE;
    }
    private void Start()
    {
        //engage startup process
        startupProcess();

        //engage post-startup Process
        postStartupProcess();
    }
    /// <summary>
    /// Invokes the onStartupProcess methods of the onStartupProcess event subscribers
    /// </summary>
    private void startupProcess()
    {
        //invoke the event subscribers' onStartupProcess methods
        onStartupProcess.Invoke();
    }
    /// <summary>
    /// Invokes the onPostStartProcess methods of the onPostStartProcess event subscribers
    /// </summary>
    private void postStartupProcess()
    {
        //invoke the event subscribers' onPostStartProcess methods
        onPostStartupProcess.Invoke();
    }
    /// <summary>
    /// Sets the network status of the appication
    /// </summary>
    /// <param name="networkStatus"></param>
    public void setNetworkStatus(NetworkStatus networkStatus)
    {
        this.networkStatus = networkStatus; 
    }

    /// <summary>
    /// method to check if the application's network status is set to ONLINE.
    /// </summary>
    /// <returns></returns>
    public bool isOnline()
    {
        return networkStatus == NetworkStatus.ONLINE;
    }

    /// <summary>
    /// Calculates the target position to instantiate the prefab so that the object is in front of the user
    /// </summary>
    /// <returns>targetPosition<Vector3>: the target position for the object to spawn at</returns>
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