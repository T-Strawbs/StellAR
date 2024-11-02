using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VuforiaManager : Singleton<VuforiaManager>
{
    public static bool VuforiaTargetFound = false;
    public DefaultObserverEventHandler VuforiaTarget;
    public static GameObject networkOriginObject;

    private void Awake()
    {
        // initialise origin over the vuforia image target
        networkOriginObject = new GameObject("NetworkOriginObject");
        networkOriginObject.transform.position = VuforiaTarget.transform.position;
        networkOriginObject.transform.rotation = VuforiaTarget.transform.rotation;
        networkOriginObject.transform.SetParent(VuforiaTarget.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (VuforiaTargetFound)
        {
            networkOriginObject.transform.position = VuforiaTarget.transform.position;
            networkOriginObject.transform.rotation = VuforiaTarget.transform.rotation;
        }
    }

    // Called by the Vuforia Image Target when it is found
    public void SetVuforiaTarget()
    {
        VuforiaTargetFound = true;
        DebugConsole.Instance.LogDebug("Vuforia image target found.");
    }

}
