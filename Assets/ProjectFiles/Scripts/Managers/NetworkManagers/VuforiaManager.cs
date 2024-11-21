using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class that handles the intergrated Vuforia system to provide our application with a global point of 
/// origin that that shared across all connected clients of a server to maintain the position and
/// cardinality of our networked interactables no matter where the end user is located in the
/// real world.
/// </summary>
public class VuforiaManager : Singleton<VuforiaManager>
{
    /// <summary>
    /// Flag for signalling if Vuforia has found our Vuforia target was found or not
    /// </summary>
    public static bool vuforiaTargetFound = false;
    /// <summary>
    /// The image of the VuforiaTarget that Vuforia Needs to set our global point of origin.
    /// </summary>
    public DefaultObserverEventHandler vuforiaTarget;
    /// <summary>
    /// The Gameobject we use to parent all networked interactables for all clients.
    /// Will be positioned above the Vuforia target when the target
    /// is found.
    /// </summary>
    public static GameObject networkOriginObject;

    private void Awake()
    {
        // initialise origin over the vuforia image target
        networkOriginObject = new GameObject("NetworkOriginObject");
        networkOriginObject.transform.position = vuforiaTarget.transform.position;
        networkOriginObject.transform.rotation = vuforiaTarget.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //check if our target has been found yet
        if (vuforiaTargetFound)
        {
            //reposition the networkOriginObject to ensure that it sits directly above
            //the Vuforia target.
            networkOriginObject.transform.position = vuforiaTarget.transform.position;
            networkOriginObject.transform.rotation = vuforiaTarget.transform.rotation;
        }
    }

    /// <summary>
    /// Sets the vuforiaTargetFound flag to true when the Vuforia system
    /// detects the vuforiaTarget.
    /// </summary>
    public void SetVuforiaTarget()
    {
        vuforiaTargetFound = true;
        DebugConsole.Instance.LogDebug("Vuforia image target found.");
    }

}
