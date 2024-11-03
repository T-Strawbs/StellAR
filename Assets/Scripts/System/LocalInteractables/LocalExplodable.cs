using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalExplodable : Explodable
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
                if(current.explodable is LocalExplodable currentExplodable && child.explodable is LocalExplodable childExplodable)
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
}
