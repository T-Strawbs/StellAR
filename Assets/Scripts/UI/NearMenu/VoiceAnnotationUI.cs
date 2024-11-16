using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
///

/// <summary>
/// concrete class for Voice Annotation UI elements.
/// </summary>
public class VoiceAnnotationUI : AnnotationUI
{
    /// <summary>
    /// The Audioplayer object for playing back the voice annotation Audio.
    /// </summary>
    [SerializeField] private AudioPlayerUI audioPlayerUI;
    /// <summary>
    /// The transform of this voice annotation ui element
    /// </summary>
    [SerializeField] private RectTransform voiceUIRect;
    
    public override void initialise(AnnotationJson annotationData)
    {
        base.initialise(annotationData);
        if (annotationData is VoiceAnnotationJson voiceMessage)
        {
            // give the file path to audio clip to the audioPlayerUI
            audioPlayerUI.audioPath = voiceMessage.Content;
            audioPlayerUI.setAudioSource(null);
            
            //if offline or online as host
            if(!ApplicationManager.Instance.isOnline() || (ApplicationManager.Instance.isOnline() && NetworkManager.Singleton.IsHost))
            {
                //attempt to load audio via our load audio delegate
                AudioLoader.Instance.loadAudio(onAudioLoaded, voiceMessage.Content);
                
            }
            //if online we don't need to load audio until button is preseed

            //rebuild the ui
            LayoutRebuilder.ForceRebuildLayoutImmediate(voiceUIRect);
        }
    }

    private void onAudioLoaded(AudioClip clip,string audioPath)
    {
        //if the clip is null
        if(!clip)
        {
            DebugConsole.Instance.LogError($"Couldnt load audio from file:{audioPath}");
            return;
        }
        //its not null so we'll load the clip into our audio player
        audioPlayerUI.setAudioSource(clip);
    }
}
