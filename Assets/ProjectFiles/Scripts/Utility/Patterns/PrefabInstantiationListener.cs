using System;
using Unity;
using UnityEngine;

/// <summary>
/// Listener interface for classes that need to be notified by the prefab
/// manager when a prefab is instantiated
/// 
/// make sure to subscribe to the prefab managers OnPrefabInstantiation event in the awake method
/// private void Awake()
/// {
///     PrefabManager.Instance.OnPrefabInstantiation.AddListener(onPrefabInstantiation);
/// }
/// </summary>
public interface PrefabInstantationListener
{
    /// <summary>
    /// Invokes behaviour when a prefab has been instantiated.
    /// </summary>
    /// <param name="instance"></param>
    public void onPrefabInstantiation(GameObject instance);
}