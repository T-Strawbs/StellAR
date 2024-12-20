
using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     � Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     � Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// This is a inteface for classes that want to be updated when the prefab manager loads
/// the prefabs.
/// 
/// Make sure to subscribe to the prefab managers  OnPrefabsLoaded event in the awake method
/// 
/// private void Awake()
/// {
///     PrefabManager.Instance.OnPrefabsLoaded.AddListener(onPrefabsLoaded);    
/// }
/// 
/// </summary>
public interface PrefabLoadListener
{
    /// <summary>
    /// Invokes behaviour when the prefab manager has finished loading the prefabs.
    /// </summary>
    /// <param name="loadedPrefabs"></param>
    public void onPrefabsLoaded(List<GameObject> loadedPrefabs);
}