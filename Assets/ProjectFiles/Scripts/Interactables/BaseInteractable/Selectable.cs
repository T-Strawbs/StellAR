using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class Selectable : MonoBehaviour
{
    public Interactable interactable { get; set; }

    protected ExtendableObjectManipulator manipulator;
    public virtual ExtendableObjectManipulator Manipulator
    {
        get { return manipulator; }
        set
        {
            manipulator = value;
            manipulator.selectEntered.AddListener(onSelection);
            manipulator.selectExited.AddListener(onDeselection);
        }
    }

    /// <summary>
    /// The colldier that the object manipulator will need to be interacted with
    /// </summary>
    public MeshCollider meshCollider;

    protected abstract void onSelection(SelectEnterEventArgs args);
    protected abstract void onDeselection(SelectExitEventArgs args);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isAllowed"></param>
    public virtual void toggleAllowedManipulations(bool isAllowed)
    {
        if (isAllowed)
        {
            //allow all manipulations
            manipulator.AllowedManipulations =
                    MixedReality.Toolkit.TransformFlags.Move |
                    MixedReality.Toolkit.TransformFlags.Rotate |
                    MixedReality.Toolkit.TransformFlags.Scale;
        }
        else
        {
            //revoke all manipulations
            manipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.None;
        }
    }
}