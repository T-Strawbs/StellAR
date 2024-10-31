using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

enum AnimBtnState
{
    PLAY,
    STOP,
    RESET
}

public class AnimationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text content;
    [SerializeField] private Animation contentAnimation;
    [SerializeField] private PressableButton animationBtn;
    [SerializeField] private AnimBtnState btnState;

    public void populateContent(Animation animation,AnimationClip animationClip,string animationName)
    {
        //set the content text
        content.text = animationName;
        //set the current animation component
        contentAnimation = animation;
        //set the animations clip
        contentAnimation.clip = animationClip;
        //set btn state to Play
        btnState = AnimBtnState.PLAY;
    }

    public void toggleAnimation()
    {
        if (contentAnimation == null || !contentAnimation.GetClip(content.text))
        {
            DebugConsole.Instance.LogError($"{content.text} has no animation to play");
            return;
        }
        switch (btnState)
        {
            case AnimBtnState.PLAY:
                //set the direction of playback to animate from the start of the clip
                setAnimationDirection(true);
                //play the animation
                DebugConsole.Instance.LogDebug($"Playing animation: {content.text}");
                contentAnimation.Play(content.text);
                btnState = AnimBtnState.STOP;
                return;
            case AnimBtnState.STOP:
                if(contentAnimation.isPlaying)
                {
                    //stop animation
                    DebugConsole.Instance.LogDebug($"Stopping animation: {content.text}");
                    contentAnimation.Stop();
                    return;
                }
                //reverse the anim playback to reset the objects transfroms
                setAnimationDirection(false);
                //play in reverse
                contentAnimation.Play(content.text);
                //set btn state to play
                btnState = AnimBtnState.PLAY;
                return;
            case AnimBtnState.RESET:
                //reverse the anim playback to reset the objects transforms
                setAnimationDirection(false);
                //play in reverse
                contentAnimation.Play(content.text);
                //set btn state to play
                btnState = AnimBtnState.PLAY;
                return;
        } 
    
    }
    private void setAnimationDirection(bool playFoward)
    {
        AnimationState animState = contentAnimation[content.text];
        if(!animState)
        {
            DebugConsole.Instance.LogError($"cant set the anim speed as the state of animation: {content.text} cannot be found");
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


