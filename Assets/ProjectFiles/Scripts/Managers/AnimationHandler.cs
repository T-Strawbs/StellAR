using MixedReality.Toolkit.UX;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class for managing the Animation components of the currently selected interactable and populating 
/// the AnimationPane UI element.
/// </summary>
public class AnimationHandler : Singleton<AnimationHandler>, NewSelectionListener
{
    /// <summary>
    /// The AnimationPane UI element that displays all the animations for the currently
    /// selected model
    /// </summary>
    [SerializeField] private AnimationPane animationPane;

    private void Awake()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(onNewSelection);
    }

    public void onNewSelection(Transform selection)
    {
        string parentName = SelectionManager.Instance.getModelRoot(SelectionManager.Instance.currentSelection.transform).name;

        if (string.IsNullOrEmpty(parentName))
        {
            DebugConsole.Instance.LogWarning($"couldnt find parent name for component:{selection.name}");
            //set the animation pane's current animations as an empty list
            animationPane.CurrentAnimation = null;
            //populate the animation pane
            animationPane.populateScrollablePane(null);
            return;
        }
        //grab the current selections animation component
        Animation animationComponent = selection.GetComponent<Animation>();
        if (animationComponent == null)
        {
            DebugConsole.Instance.LogWarning($"couldnt find animation for model:{parentName}");
            //set the animation pane's current animations as an empty list
            animationPane.CurrentAnimation = null;
            //populate the animation pane
            animationPane.populateScrollablePane(null);
            return;
        }
        //set the animation panes current animations as animations list
        animationPane.CurrentAnimation = animationComponent;
        //populate the animation pane
        animationPane.populateScrollablePane(null);
    }

    /// <summary>
    /// method for getting all animation clips of an animation component.
    /// </summary>
    /// <param name="animation"></param>
    /// <returns></returns>
    public static List<AnimationClip> getAllAnimationClips(Animation animation)
    {
        List<AnimationClip> animationClips = new List<AnimationClip>();
        foreach(AnimationState state in animation)
        {
            animationClips.Add(state.clip);
        }
        return animationClips;
    }

    
}
