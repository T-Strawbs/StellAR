using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class representing the UI element for displaying one of the currently selected Model's Animation clip.
/// </summary>
public class AnimationUI : MonoBehaviour
{
    /// <summary>
    /// the Text UI element for displaying the name of the animation clip
    /// </summary>
    [SerializeField] private TMP_Text animationName;
    /// <summary>
    /// The animation that will play when the animationBtn is pressed
    /// </summary>
    [SerializeField] private Animation contentAnimation;
    /// <summary>
    /// The button UI element for this animation UI element
    /// </summary>
    [SerializeField] private PressableButton animationBtn;
    /// <summary>
    /// the current state of the Animation UI to track if it's
    /// animation is playing, stopped or will reset the animation  
    /// back to the start.
    /// </summary>
    [SerializeField] private AnimBtnState btnState;
    private void Start()
    {
        //add the toggleAnimation method as an event listener that
        //will activate when the button is clicked.
        animationBtn.OnClicked.AddListener(toggleAnimation);
    }
    /// <summary>
    /// method for popluating this UI element with animation data
    /// </summary>
    /// <param name="animation"></param>
    /// <param name="animationClip"></param>
    /// <param name="animationName"></param>
    public void populateContent(Animation animation,AnimationClip animationClip,string animationName)
    {
        //set the animationName text
        this.animationName.text = animationName;
        //set the current animation component
        contentAnimation = animation;
        //set the animations clip
        contentAnimation.clip = animationClip;
        //set btn state to Play
        btnState = AnimBtnState.PLAY;
    }
    /// <summary>
    /// Method for changing the state of this animation UI element to play, stop or 
    /// reset the animation.
    /// </summary>
    private void toggleAnimation()
    {
        //check if we have an animation and if the animation contations a clip with 
        //the same name as our animationName
        if (contentAnimation == null || !contentAnimation.GetClip(animationName.text))
        {
            DebugConsole.Instance.LogError($"{animationName.text} has no animation to play");
            //it doesnt so we return
            return;
        }
        //modify the animation state
        switch (btnState)
        {
            case AnimBtnState.PLAY:
                //set the direction of playback to animate from the start of the clip
                setAnimationDirection(true);
                //play the animation
                DebugConsole.Instance.LogDebug($"Playing animation: {animationName.text}");
                contentAnimation.Play(animationName.text);
                btnState = AnimBtnState.STOP;
                return;
            case AnimBtnState.STOP:
                if(contentAnimation.isPlaying)
                {
                    //stop animation
                    DebugConsole.Instance.LogDebug($"Stopping animation: {animationName.text}");
                    contentAnimation.Stop();
                    return;
                }
                //reverse the anim playback to reset the objects transfroms
                setAnimationDirection(false);
                //play in reverse
                contentAnimation.Play(animationName.text);
                //set btn state to play
                btnState = AnimBtnState.PLAY;
                return;
            case AnimBtnState.RESET:
                //reverse the anim playback to reset the objects transforms
                setAnimationDirection(false);
                //play in reverse
                contentAnimation.Play(animationName.text);
                //set btn state to play
                btnState = AnimBtnState.PLAY;
                return;
        } 
    
    }
    /// <summary>
    /// method for manipulating the playback of the content animation depending on if
    /// we want to play the clip forward or in reverse to rewind it.
    /// </summary>
    /// <param name="playFoward"></param>
    private void setAnimationDirection(bool playFoward)
    {
        AnimationState animState = contentAnimation[animationName.text];
        if(!animState)
        {
            DebugConsole.Instance.LogError($"cant set the anim speed as the state of animation: {animationName.text} cannot be found");
            return;
        }
        if(playFoward)
        {
            //set the speed to normal
            animState.speed = 1f;
            //set the time to start from the beginning
            animState.time = 0;
            return;
        }
        //reverse the speed
        animState.speed = -10f;
    }
}
/// <summary>
/// Enum for managing the animation button's current state.
/// </summary>
enum AnimBtnState
{
    PLAY,
    STOP,
    RESET
}

