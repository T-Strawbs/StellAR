using Microsoft.MixedReality.OpenXR;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MessageBasedExplodable : Explodable
{
    private void Start()
    {
        initialTransform = new ExplodableTransform
        {
            initialPosition = transform.localPosition,
            initialRotation = transform.localRotation,
            initialScale = transform.localScale,
            objectCentre = getObjectCentre()
        };
    }
    public override void explode()
    {
        explode(interactable);
    }
    protected override void explode(Interactable current)
    {
        //Check if the current object is explodable
        if (current.explodable.explosionStatus != ExplosionStatus.EXPLODABLE)
        {
            DebugConsole.Instance.LogDebug($"Cannot explode MessageBasedInteractable {current.name} " +
                $"as its not explodable as it is {current.explodable.explosionStatus}");
            return;
        }

        //set the explosion status to exploded
        current.explodable.explosionStatus = ExplosionStatus.EXPLODED;

        //turn off the selectables object manipulator to later reset the
        //object manipulator's bounds

        current.selectable.Manipulator.enabled = false;

        foreach (Interactable child in current.children)
        {
            //deparent child
            child.transform.SetParent(null);

            //turn on child object manipulator
            //child.selectable.toggleObjectManipulatorRpc(true);
            child.selectable.Manipulator.enabled = true;

            //check if the child is not a leaf
            //if (child.explosionStatus != ExplosionStatus.LEAF)
            if (child.children.Count > 0)
            {
                //set explodable status to explodable
                child.explodable.explosionStatus = ExplosionStatus.EXPLODABLE;
            }
            //check if the child has a mesh
            if (child.meshType == MeshType.MESH)
            {
                if(current.explodable is MessageBasedExplodable currentExplodable && child.explodable is MessageBasedExplodable childExplodable)
                {
                    //calculate explosion trajectory
                    Vector3 explostionTrajectory = currentExplodable.calculateTrajectory(child.explodable);
                    //move child
                    childExplodable.invokeExplosionTranslation(explostionTrajectory);
                }
            }
            else //child is a non mesh object
            {
                //recursively explode so that the end user doesnt have to spam
                //select and explode on non mesh objects repeatedly
                explode(child);
            }

        }
        //check if the current interactable has a mesh 
        if (current.meshType == MeshType.MESH)
            //turn the current interactable's object manipulator on to reset the bounds
            // of the object manipulator 
            current.selectable.Manipulator.enabled = true;
    }


    public bool areCollapsablesOwnedSingle()
    {
        //get the imediate parent of this object
        MessageBasedInteractable parent = (MessageBasedInteractable)interactable.parent;
        if(!parent)
            return false;
        //check if the parent has exploded as you cant collapse something that hasnt exploded
        if(parent.explodable.explosionStatus != ExplosionStatus.EXPLODED)
        {
            DebugConsole.Instance.LogDebug(
                    $"{parent.name} is a parent of {name} and for some reason isnt exploded as " +
                    $"its {parent.explodable.explosionStatus}");
            return false;
        }
        //recursively check decendants if any of them are owned by a client
        return canCollapseCheckOwnership(parent);
    }

    public bool areCollapsablesOwnedAll()
    {
        //intialise a var for referencing the root MessageBasedInteractable
        MessageBasedInteractable rootInteractable = (MessageBasedInteractable)interactable;
        //recursively find the root 
        while(rootInteractable.parent != null)
        {
            rootInteractable = (MessageBasedInteractable)rootInteractable.parent;
        }
        //check if the root has exploded as you cant collapse something that hasnt exploded
        if (rootInteractable.explodable.explosionStatus != ExplosionStatus.EXPLODED)
        {
            DebugConsole.Instance.LogDebug(
                    $"{rootInteractable.name} is a the root object of {name} and for some reason isnt exploded as " +
                    $"its {rootInteractable.explodable.explosionStatus}");
            return false;
        }

        //recursively check decendants if any of them are owned by a client
        return canCollapseCheckOwnership(rootInteractable);
    }

    private bool canCollapseCheckOwnership(MessageBasedInteractable currentInteractable)
    {
        //for each child of the current interactable
        foreach (MessageBasedInteractable child in currentInteractable.children)
        {
            //check if the child is able to explode or is a leaf
            if(child.explosionStatus() == ExplosionStatus.EXPLODABLE || 
                child.explosionStatus() == ExplosionStatus.LEAF)
            {
                //check if its owned and if true, return false
                if(child.isOwned())
                    return false;
            }
            //check if child has exploded
            else if (child.explosionStatus() == ExplosionStatus.EXPLODED)
            {
                //recursively dig into the childs decendants as we're
                //really only looking for children that are leaves or explodables
                bool _canCollapse = canCollapseCheckOwnership(child);
                //if the child cant collapse return
                if(!_canCollapse)
                    return false;
            }
        }
        //every check was fine so return true
        return true;
    }

    public bool areCollapsablesLockedSingle()
    {
        //get the imediate parent of this object
        MessageBasedInteractable parent = (MessageBasedInteractable)interactable.parent;
        if (!parent)
        {
            DebugConsole.Instance.LogDebug($"Hello world");
            return false;
        }
            
        //check if the parent has exploded as you cant collapse something that hasnt exploded
        if (parent.explodable.explosionStatus != ExplosionStatus.EXPLODED)
        {
            DebugConsole.Instance.LogDebug(
                    $"{parent.name} is a parent of {name} and for some reason isnt exploded as " +
                    $"its {parent.explodable.explosionStatus}");
            return false;
        }
        //recursively check decendants if any of them are owned by a client
        return areCollapsablesLocked(parent);
    }

    public bool areCollapsablesLockedALL()
    {
        //intialise a var for referencing the root MessageBasedInteractable
        MessageBasedInteractable rootInteractable = (MessageBasedInteractable)interactable;
        //recursively find the root 
        while (rootInteractable.parent != null)
        {
            rootInteractable = (MessageBasedInteractable)rootInteractable.parent;
        }
        //check if the root has exploded as you cant collapse something that hasnt exploded
        if (rootInteractable.explodable.explosionStatus != ExplosionStatus.EXPLODED)
        {
            DebugConsole.Instance.LogDebug(
                    $"{rootInteractable.name} is a the root object of {name} and for some reason isnt exploded as " +
                    $"its {rootInteractable.explodable.explosionStatus}");
            return false;
        }

        //recursively check decendants if any of them are owned by a client
        return areCollapsablesLocked(rootInteractable);
    }

    private bool areCollapsablesLocked(MessageBasedInteractable currentInteractable)
    {
        //for each child of the current interactable
        foreach (MessageBasedInteractable child in currentInteractable.children)
        {
            //check if the child is able to explode or is a leaf
            if (child.explosionStatus() == ExplosionStatus.EXPLODABLE ||
                child.explosionStatus() == ExplosionStatus.LEAF)
            {
                //check if its owned and if true, return false
                if (child.isLocked())
                {
                    DebugConsole.Instance.LogDebug($"{child.name} is locked apparently: {child.getOwnerID() == GlobalConstants.OWNERSHIP_LOCKED}");
                    return true;
                }
            }
            //check if child has exploded
            else if (child.explosionStatus() == ExplosionStatus.EXPLODED)
            {
                //recursively dig into the childs decendants as we're
                //really only looking for children that are leaves or explodables
                bool isLocked = areCollapsablesLocked(child);
                //if the child cant collapse return
                if (isLocked)
                    return true;
            }
        }
        //every check was fine so return true
        return false;
    }


}
