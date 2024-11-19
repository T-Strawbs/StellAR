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
/// interface for listening to the Selection Manager's onLocalSelectionChanged event
/// 
/// make sure to subscribe to the event in the awake method
/// private void Awake()
/// {
///     SelectionManager.Instance.onLocalSelectionChanged.AddListener(onNewSelectionListener);   
/// }
/// </summary>
public interface NewSelectionListener
{
    /// <summary>
    /// Invokes behaviour when the onLocalSelectionChanged event is called
    /// </summary>
    /// <param name="selection"></param>
    public void onNewSelection(Transform selection);
}
