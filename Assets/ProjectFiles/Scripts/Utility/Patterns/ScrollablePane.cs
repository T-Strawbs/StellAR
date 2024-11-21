using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Interface for objects that contain UI elements in a scrollable element like the Unity ScrollView component.
/// </summary>
public interface ScrollablePane
{
    /// <summary>
    /// Method for populating the scrollable container with UI elements using the 
    /// </summary>
    /// <param name="loadedPrefabs"></param>
    public void populateScrollablePane(List<GameObject> loadedPrefabs);
}