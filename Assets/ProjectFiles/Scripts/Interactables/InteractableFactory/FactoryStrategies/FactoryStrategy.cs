using System;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Interface for strategies that initialise gameobjects as interactables
/// </summary>
public interface FactoryStrategy
{
    /// <summary>
    /// initialises the passed gameobject as an interactable of a given type.
    /// </summary>
    /// <param name="interactableObject"></param>
    public void initialiseInteractable(GameObject interactableObject);
}