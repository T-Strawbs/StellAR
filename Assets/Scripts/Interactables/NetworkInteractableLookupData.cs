using System;
using UnityEngine;
using Unity.Netcode;

public struct NetworkInteractableLookupData 
{
    /// <summary>
    /// The name of the root object for this interactable
    /// </summary>
    public int parentKey;
    /// <summary>
    /// The name of this interactable
    /// </summary>
    public int objectIndex;
}