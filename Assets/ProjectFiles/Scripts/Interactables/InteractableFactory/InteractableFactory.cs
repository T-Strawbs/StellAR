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
/// Factory Pattern for initialising interactables based on their types.
/// </summary>
public class InteractableFactory : Singleton<InteractableFactory>
{
    /// <summary>
    /// Dictionary for containing factory strategies.
    /// </summary>
    private Dictionary<InteractableType,FactoryStrategy> factoryStrategies = 
        new Dictionary<InteractableType,FactoryStrategy>();

    private void Awake()
    {
        //add the strategies
        initialiseStrategies();
    }

    /// <summary>
    /// initialiser method for creating factory strategies and adding them to our dictionary
    /// </summary>
    private void initialiseStrategies()
    {
        //add the strategy for..
        //Local interactables
        LocalBasedStrategy localBasedStrategy = new LocalBasedStrategy();
        factoryStrategies.Add(InteractableType.LocalBased, localBasedStrategy);
        //message based interactables
        MessagedBasedStrategy messagedBasedStrategy = new MessagedBasedStrategy();
        factoryStrategies.Add(InteractableType.MessageBased,messagedBasedStrategy);
    }

    /// <summary>
    /// method for making a model instance an interactable based on the desired type
    /// </summary>
    /// <param name="interactableType"></param>
    /// <param name="interactableObject"></param>
    public void initialiseInteractable(InteractableType interactableType,GameObject interactableObject)
    {
        //make the instance an interactable based on the interactableType
        switch(interactableType)
        {
            case InteractableType.LocalBased:
                factoryStrategies[InteractableType.LocalBased].initialiseInteractable(interactableObject);
                break;
            case InteractableType.MessageBased:
                factoryStrategies[InteractableType.MessageBased].initialiseInteractable(interactableObject);
                break;
        }
    }

}

public enum InteractableType 
{ 
    LocalBased, 
    MessageBased,
}
