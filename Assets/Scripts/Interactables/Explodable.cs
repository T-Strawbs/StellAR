using Microsoft.MixedReality.OpenXR;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public enum ExplosionStatus { EXPLODABLE, EXPLODED, INACTIVE, LEAF }

public struct ExplodableTransform
{
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public Vector3 initialScale;
    public Vector3 objectCentre;
}
public class Explodable : MonoBehaviour
{
    public Interactable interactable { get; set; }

    public ExplodableTransform initialTransform;//{ get; set; }
    public ExplosionStatus explosionStatus;// { get; set; }


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
    public void explode()
    {
        explode(interactable);
    }
    private void explode(Interactable current)
    {
        //Check if the current object is explodable
        if (current.explodable.explosionStatus != ExplosionStatus.EXPLODABLE)
        {
            DebugConsole.Instance.LogDebug($"Cannot explode Interactable {current.name} " +
                $"as its not explodable as it is {current.explodable.explosionStatus}");
            return;
        }

        //set the explosion status to exploded
        current.explodable.explosionStatus = ExplosionStatus.EXPLODED;

        //turn off the selectables object manipulator to later reset the
        //object manipulator's bounds

        current.selectable.manipulator.enabled = false;

        foreach (Interactable child in current.children)
        {
            //deparent child
            child.transform.SetParent(null);

            //turn on child object manipulator
            //child.selectable.toggleObjectManipulatorRpc(true);
            child.selectable.manipulator.enabled = true;

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
                //calculate explosion trajectory
                Vector3 explostionTrajectory = current.explodable.calculateTrajectory(child.explodable);
                //move child
                child.explodable.invokeExplosionTranslation(explostionTrajectory);
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
            current.selectable.manipulator.enabled = true;

    }

    private Vector3 calculateTrajectory(Explodable child)
    {
        //get the default positions if we dont have renderers
        Vector3 childCentre = child.initialTransform.objectCentre;
        Vector3 parentCentre = initialTransform.objectCentre;

        //calculate trajectory
        Vector3 trajectory = childCentre - parentCentre;

        return trajectory;
    }
    private Vector3 getObjectCentre()
    {
        //the bounds of the renderer
        MeshRenderer currentMeshRenderer = GetComponent<MeshRenderer>();
        SkinnedMeshRenderer currentSkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        //check if we have a mesh renderer on the current object
        if (currentMeshRenderer)
        {
            //return the centre of the mesh renderer bounds
            return currentMeshRenderer.bounds.center;
        }
        //check if the object has a skinned mesh renderer instead
        else if (currentSkinnedMeshRenderer)
        {
            //return the centre of the skinned mesh renderer bounds
            return currentSkinnedMeshRenderer.bounds.center;
        }
        //return the position of the current object
        return transform.position;
    }

    private void invokeExplosionTranslation(Vector3 explosionTrajectory)
    {
        StartCoroutine(translateExplosion(explosionTrajectory));
    }

    private IEnumerator translateExplosion(Vector3 explosionTrajectory)
    {
        //turn off the object manipualtors of the current explodable and descendants
        toggleDescendantManipulation(interactable, false);
        //grab the intial position of this explodable before we translate
        Vector3 initialPosition = transform.localPosition;
        //intialise the target position for this object to end up
        Vector3 targetPosition =
            initialPosition + transform.InverseTransformDirection(explosionTrajectory).normalized
            * GlobalConstants.EXPLOSION_STOP_DISTANCE;
        //initialise the elapsedTime that we'll use to track how long we have translated for
        float elapsedTime = 0f;
        //set the maximum duration of the translation 
        float duration = GlobalConstants.EXPLOSION_STOP_DISTANCE / GlobalConstants.TRANSLATION_SPEED;
        //while we havent reached the maximum duration
        while (elapsedTime < duration)
        {
            //translate this object using relative transforms so it moves relevent to its
            //parent/originial local position. Makes collapsing easier.
            transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            //increment the elasped time by the time difference from last frame to this frame (delta)
            elapsedTime += Time.deltaTime;
            //we arent done yet so we "await" the next frame
            yield return null;// new WaitForEndOfFrame();
        }
        //translation should be done so we can enable manipulations of this exploble and descendants again
        toggleDescendantManipulation(interactable, true);

    }

    public void collapseSingle()
    {
        //grab the this interactables parent 
        Interactable predecessorParent = (Interactable) interactable.parent;
        //check if the predecessor was actually the root this whole time
        if (!predecessorParent)
        {
            //turn off the roots object manipulator for bounds recalculation
            interactable.selectable.manipulator.enabled = false;
            //it was the root so we collapse from the root down
            collapseChildren(interactable);
            //set the root's status to explodable
            interactable.explodable.explosionStatus = ExplosionStatus.EXPLODABLE;
            //turn its object manipulator back on
            interactable.selectable.manipulator.enabled = true;
        }
        else //the predecessor wasnt the root
        {
            //turn off the parent's object manipulator for bounds recalculation
            predecessorParent.selectable.manipulator.enabled = false;
            //recursivelly collapse the predecessor parent's descendants
            collapseChildren(predecessorParent);
            //set the parent's status to explodable
            predecessorParent.explodable.explosionStatus = ExplosionStatus.EXPLODABLE;
            //turn its object manipulator back on
            predecessorParent.selectable.manipulator.enabled = true;
        }
    }
    public void collapseAll()
    {
        //Find the root explodable
        Interactable rootExplodable = interactable;
        while (rootExplodable.parent != null)
        {
            rootExplodable = (Interactable)rootExplodable.parent;
        }
        //turn off the roots object manipulator to recalculate its bounds
        rootExplodable.selectable.manipulator.enabled = false;
        //collapse the descendants of the root
        collapseChildren(rootExplodable);
        //set the root's status to explodable
        rootExplodable.explodable.explosionStatus = ExplosionStatus.EXPLODABLE;
        //check if its a non mesh object
        if (rootExplodable.meshType == MeshType.NON_MESH)
            //turn its object manipulator back on
            rootExplodable.selectable.manipulator.enabled = true;
    }

    [Obsolete("This method was used back when we had all objectmanipulators turn on for all interactables at once.")]
    private Interactable findInteractablePredecessor(Interactable currentInteractable)
    {
        //check if the parent of the current interactable is null
        if (!currentInteractable.parent)
            //then this is root so return
            return currentInteractable;
        //check if the current explodable is a leaf 
        if (currentInteractable.explodable.explosionStatus == ExplosionStatus.LEAF)
            //the current explodable is a leaf as its immediate parent has exploded already
            return currentInteractable;
        //check if the parents explodable is able to explode
        Interactable parent = (Interactable)currentInteractable.parent;
        if (parent.explodable.explosionStatus == ExplosionStatus.EXPLODABLE)
            //return this parents  explodable as its the highest predecessor that is explodable
            return (Interactable)currentInteractable.parent;
        //the parent's explodable is not explodable so we climb higher up the tree to find a parent that is
        return findInteractablePredecessor((Interactable)currentInteractable.parent);
    }

    private void collapseChildren(Interactable parent)
    {
        //for each child of the current parent
        foreach (Interactable child in parent.children)
        {
            //check if the child has exploded
            if (child.explodable.explosionStatus == ExplosionStatus.EXPLODED)
                //collapse the childs descendants
                collapseChildren(child);

            //check if the child is a not a leaf
            if(child.children.Count > 1)
            {
                // its not so set the child's explosion status to inactive
                child.explodable.explosionStatus = ExplosionStatus.INACTIVE;
            }
            
            //turn off the childs object manipulator 
            child.selectable.manipulator.enabled = false;
            //set the child's transform parent back to its parent again
            child.transform.SetParent(child.parent.transform);
            //move the child back to its parent
            child.explodable.invokeCollapse();
        }

    }

    private void invokeCollapse()
    {
        StartCoroutine(translateCollapse());
    }

    private IEnumerator translateCollapse()
    {
        //turn off the object manipualtors of the current explodable and descendants
        toggleDescendantManipulation(interactable, false);
        //set the initial position of this translation
        Vector3 initialPosition = transform.localPosition;
        //set the intial rotation 
        Quaternion initialRotation = transform.localRotation;
        //set the initial scale 
        Vector3 initialScale = transform.localScale;
        //initialise the elasped time
        float elapsedTime = 0f;
        //initialise the maximum duration of the translation
        float duration = 1f / GlobalConstants.TRANSLATION_SPEED;
        //while we havent finished the translation
        while (elapsedTime < duration)
        {
            //translate the current explodable's transform back to its preexplosion relative transoform
            transform.localPosition = Vector3.Lerp(initialPosition, initialTransform.initialPosition, elapsedTime / duration);
            transform.localRotation = Quaternion.Lerp(initialRotation, initialTransform.initialRotation, elapsedTime / duration);
            transform.localScale = Vector3.Lerp(initialScale, initialTransform.initialScale, elapsedTime / duration);
            //clamp the position of the current explodable so that the bounds of its model don't freak the engine out.
            transform.localPosition = Vector3.ClampMagnitude(transform.localPosition, GlobalConstants.CLAMPING_DISTANCE);
            //increment the elapsed time by the difference in time between the last frame and current frame.
            elapsedTime += Time.deltaTime;
            //wait until next frame
            yield return null;// new WaitForEndOfFrame();
        }
        //snap the explodable to the exact preexplostion position
        //snap us to the exact pre-explosion position
        transform.localPosition = Vector3.ClampMagnitude(initialTransform.initialPosition, GlobalConstants.CLAMPING_DISTANCE);
        //same with the rotation
        transform.localRotation = initialTransform.initialRotation;
        //and scale 
        transform.localScale = initialTransform.initialScale;
        //translation should be done so we can enable manipulations of this exploble and descendants again
        toggleDescendantManipulation(interactable, true);
    }


    private void toggleDescendantManipulation(Interactable current, bool isAllowed)
    {
        current.selectable.toggleAllowedManipulations(isAllowed);
        foreach (Interactable child in current.children)
        {
            toggleDescendantManipulation(child, isAllowed);
        }
    }

    public bool canExplode()
    {
        if (explosionStatus == ExplosionStatus.EXPLODABLE)
            return true;
        return false;
    }

    public bool areCollapsablesOwnedSingle()
    {
        //get the imediate parent of this object
        Interactable parent = (Interactable)interactable.parent;
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
        //intialise a var for referencing the root Interactable
        Interactable rootInteractable = interactable;
        //recursively find the root 
        while(rootInteractable.parent != null)
        {
            rootInteractable = (Interactable)rootInteractable.parent;
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

    private bool canCollapseCheckOwnership(Interactable currentInteractable)
    {
        //for each child of the current interactable
        foreach (Interactable child in currentInteractable.children)
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
        Interactable parent = (Interactable)interactable.parent;
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
        //intialise a var for referencing the root Interactable
        Interactable rootInteractable = interactable;
        //recursively find the root 
        while (rootInteractable.parent != null)
        {
            rootInteractable = (Interactable)rootInteractable.parent;
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

    private bool areCollapsablesLocked(Interactable currentInteractable)
    {
        //for each child of the current interactable
        foreach (Interactable child in currentInteractable.children)
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
