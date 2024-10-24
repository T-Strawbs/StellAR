using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// class that manages the 
/// </summary>
public class ApplicationManager : Singleton<ApplicationManager>
{

    [NonSerialized] public UnityEvent onProcessCustomMessengers = new UnityEvent();

    [NonSerialized] public UnityEvent onPostStartProcess = new UnityEvent();

    private void Start()
    {
        startupProcess();
    }

    private void startupProcess()
    {
        //setup custom message handlers
        onProcessCustomMessengers.Invoke();
        //start the server to run locally
        onPostStartProcess.Invoke();
    }
}
