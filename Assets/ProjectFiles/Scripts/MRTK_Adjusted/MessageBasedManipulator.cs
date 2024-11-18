using MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MessageBasedManipulator : ExtendableObjectManipulator
{
    public MessageBasedInteractable networkInteractble { get; set; }

    protected override void Awake()
    {
        base.Awake();

        networkInteractble = GetComponent<MessageBasedInteractable>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

    }

    protected override void ApplyTargetTransform()
    {
        //check if we have ownership
        if(!networkInteractble.isOwned())
        {
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
             * attached in our case we dont and shouldnt
          
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
