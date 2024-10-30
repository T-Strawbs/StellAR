using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class VoiceAnnotationUI : AnnotationUI
{
    [SerializeField] private AudioPlayerUI audioPlayerUI;

    [SerializeField] private RectTransform voiceUIRect;
    
    public override void initialise(AnnotationJson annotationData)
    {
        base.initialise(annotationData);
        if (annotationData is VoiceAnnotationJson voiceMessage)
        {
            //if offline
            if(!ApplicationManager.Instance.isOnline())
            {
                //attempt to load audio via our load audio delegate
                AudioLoader.Instance.loadAudio(onAudioLoaded, voiceMessage.Content);
            }
            //is online
            else
            {
                //send Rpc to server to load the audio
                AudioLoader.Instance.loadAudioOnlineServerRpc(voiceMessage.Content, NetworkManager.Singleton.LocalClientId);
            }
        }
        //rebuild the ui
        LayoutRebuilder.ForceRebuildLayoutImmediate(voiceUIRect);
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
