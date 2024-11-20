using MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Custom ObjectManipulator implementation that synchronises its tranform and manipulations across the 
/// network.
/// </summary>
public class MessageBasedManipulator : ExtendableObjectManipulator
{
    /// <summary>
    /// Reference to the MessageBased Interactable that this MessageBased Manipulator is 
    /// </summary>
    public MessageBasedInteractable networkInteractble { get; set; }

    protected override void Awake()
    {
        base.Awake();

        networkInteractble = GetComponent<MessageBasedInteractable>();
    }
    /// <summary>
    /// Override for applying new transform data when this object is manipulated.
    /// </summary>
    protected override void ApplyTargetTransform()
    {
        //check if we have ownership
        if(!networkInteractble.isOwned())
        {
            //we dont so request ownership
            networkInteractble.requestOwnership();
            return;
        }
        // modifiedTransformFlags currently unused.
        TransformFlags modifiedTransformFlags = TransformFlags.None;
        ModifyTargetPose(ref targetTransform, ref modifiedTransformFlags);

        if (rigidBody == null)
        {
            //attempt to update the transform of the network interactable
            networkInteractble.requestUpdateTransform
                (
                    targetTransform.Position, targetTransform.Rotation, targetTransform.Scale
                );
        }
        else
        {
            /* below is logic for applying the transform if this object has a rigidbody 
             * attached in our case we dont and we shouldnt
          
            // There is a Rigidbody. Potential different paths for near vs far manipulation
            if (!useForces)
            {
                rigidBody.MovePosition(targetTransform.Position);
                rigidBody.MoveRotation(targetTransform.Rotation);
            }

            HostTransform.localScale = targetTransform.Scale;
            */
        }
    }
}
