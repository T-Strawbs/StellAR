using System;
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
/// Abstract class for facilitating the exploding and collasping of interactables into individual/clusters
/// of interactable objects.
/// </summary>
public abstract class Explodable : MonoBehaviour
{
    /// <summary>
    /// reference to the interactable that this expldable belongs to.
    /// </summary>
    public Interactable interactable;
    /// <summary>
    /// the intial local transform of this interactable before it explodes
    /// </summary>
    public ExplodableTransform initialTransform;
    /// <summary>
    /// the explosion status of this explodable
    /// </summary>
    public ExplosionStatus explosionStatus;
    /// <summary>
    /// method for starting the explosion process of this interactable
    /// </summary>
    public abstract void explode();
    /// <summary>
    /// recursive method for exploding an interactable's children
    /// </summary>
    /// <param name="current"></param>
    protected abstract void explode(Interactable current);
    /// <summary>
    /// method for calculating the trajectory of this interactable for when it explodes.
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    protected virtual Vector3 calculateTrajectory(Explodable child)
    {
        //get the default positions if we dont have renderers
        Vector3 childCentre = child.initialTransform.objectCentre;
        Vector3 parentCentre = initialTransform.objectCentre;

        //calculate trajectory
        Vector3 trajectory = childCentre - parentCentre;

        return trajectory;
    }
    /// <summary>
    /// method for getting the center of this interactables mesh bounds so we can
    /// get the absolute center of this object.
    /// </summary>
    /// <returns></returns>
    protected virtual Vector3 getObjectCentre()
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
    /// <summary>
    /// method for starting the corountine for this interactable to move out from the explosion 
    /// center until they reach their destination.
    /// </summary>
    /// <param name="explosionTrajectory"></param>
    protected virtual void invokeExplosionTranslation(Vector3 explosionTrajectory)
    {
        StartCoroutine(translateExplosion(explosionTrajectory));
    }
    /// <summary>
    /// Coroutine for translating this interactable's transfrom from the position of the explosion 
    /// out until the interactable reaches its destination.
    /// </summary>
    /// <param name="explosionTrajectory"></param>
    /// <returns></returns>
    protected virtual IEnumerator translateExplosion(Vector3 explosionTrajectory)
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
    /// <summary>
    /// method for collapsing this interactable one level.
    /// </summary>
    public virtual void collapseSingle()
    {
        //grab the this interactables parent 
        Interactable predecessorParent = interactable.parent;
        //check if the predecessor was actually the root this whole time
        if (!predecessorParent)
        {
            //turn off the roots object manipulator for bounds recalculation
            interactable.selectable.Manipulator.enabled = false;
            //it was the root so we collapse from the root down
            collapseChildren(interactable);
            //set the root's status to explodable
            interactable.explodable.explosionStatus = ExplosionStatus.EXPLODABLE;
            //turn its object manipulator back on
            interactable.selectable.Manipulator.enabled = true;
        }
        else //the predecessor wasnt the root
        {
            //turn off the parent's object manipulator for bounds recalculation
            predecessorParent.selectable.Manipulator.enabled = false;
            //recursivelly collapse the predecessor parent's descendants
            collapseChildren(predecessorParent);
            //set the parent's status to explodable
            predecessorParent.explodable.explosionStatus = ExplosionStatus.EXPLODABLE;
            //turn its object manipulator back on
            predecessorParent.selectable.Manipulator.enabled = true;
        }
    }
    /// <summary>
    /// method for collapsing this interactable completey so the root model is reintact.
    /// </summary>
    public virtual void collapseAll()
    {
        //Find the root explodable
        Interactable rootExplodable = interactable;
        while (rootExplodable.parent != null)
        {
            rootExplodable = rootExplodable.parent;
        }
        //turn off the roots object manipulator to recalculate its bounds
        rootExplodable.selectable.Manipulator.enabled = false;
        //collapse the descendants of the root
        collapseChildren(rootExplodable);
        //set the root's status to explodable
        rootExplodable.explodable.explosionStatus = ExplosionStatus.EXPLODABLE;
        //check if its a non mesh object
        if (rootExplodable.meshType == MeshType.NON_MESH)
            //turn its object manipulator back on
            rootExplodable.selectable.Manipulator.enabled = true;
    }
    /// <summary>
    /// Depth first recursive method for collapsing the children of the given interactable.
    /// </summary>
    /// <param name="parent"></param>
    protected virtual void collapseChildren(Interactable parent)
    {
        //for each child of the current parent
        foreach (Interactable child in parent.children)
        {
            //check if the child has exploded
            if (child.explodable.explosionStatus == ExplosionStatus.EXPLODED)
                //collapse the childs descendants
                collapseChildren(child);

            //check if the child is a not a leaf
            if (child.children.Count > 1)
            {
                // its not so set the child's explosion status to inactive
                child.explodable.explosionStatus = ExplosionStatus.INACTIVE;
            }

            //turn off the childs object manipulator 
            child.selectable.Manipulator.enabled = false;
            //set the child's transform parent back to its parent again
            child.transform.SetParent(child.parent.transform);
            //move the child back to its parent
            child.explodable.invokeCollapse();
        }
    }
    /// <summary>
    /// method for starting the coroutine thats moving this interactable back to its
    /// intial position prior to the explosion.
    /// </summary>
    protected virtual void invokeCollapse()
    {
        StartCoroutine(translateCollapse());
    }
    /// <summary>
    /// Coroutine for translating the transform of this interactable back to its initial transform
    /// prior to explosion.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator translateCollapse()
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
    /// <summary>
    /// method for togging the manipulation of the interactable so that we can deactivate it
    /// when translating and reactivating when we're done.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="isAllowed"></param>
    protected virtual void toggleDescendantManipulation(Interactable current, bool isAllowed)
    {
        current.selectable.toggleAllowedManipulations(isAllowed);
        foreach (Interactable child in current.children)
        {
            toggleDescendantManipulation(child, isAllowed);
        }
    }
    /// <summary>
    /// method for checking if this interactable can explode
    /// </summary>
    /// <returns></returns>
    public bool canExplode()
    {
        if (explosionStatus == ExplosionStatus.EXPLODABLE)
            return true;
        return false;
    }
}
/// <summary>
/// Data struct for holding a reference of an Explodable's initial transform 
/// prior to explosion.
/// </summary>
public struct ExplodableTransform
{
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    public Vector3 initialScale;
    public Vector3 objectCentre;
}
/// <summary>
/// Enum for marking the status of whether an Explodable can explode or not.
/// </summary>
public enum ExplosionStatus { EXPLODABLE, EXPLODED, INACTIVE, LEAF }