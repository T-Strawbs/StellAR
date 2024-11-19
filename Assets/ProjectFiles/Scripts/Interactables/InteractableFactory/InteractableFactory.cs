using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableFactory : Singleton<InteractableFactory>
{
    private Dictionary<InteractableType,FactoryStrategy> factoryStrategies = 
        new Dictionary<InteractableType,FactoryStrategy>();

    private void Awake()
    {
        //add the strategies
        initialiseStrategies();
    }

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

    public void initialiseInteractable(InteractableType interactableType,GameObject interactableObject)
    {
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
