using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceAnnotationUI : AnnotationUI
{
    [SerializeField] private AudioPlayerUI audioPlayerUI;

    public override void initialise(AnnotationJson annotationData)
    {
        base.initialise(annotationData);
        if (annotationData is VoiceAnnotationJson voiceMessage)
        {
            //attempt to load audio
            StartCoroutine(audioPlayerUI.loadAudio(voiceMessage.AudioPath));
        }
    }
}
