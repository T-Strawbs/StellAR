using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class VoiceAnnotationUI : AnnotationUI
{
    [SerializeField] private string audioPath;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private PressableButton playButton;
    [SerializeField] private Slider seekSlider;

    [SerializeField] private bool isPlaying = false;

    [SerializeField] private TMP_Text currentAudioTime;
    [SerializeField] private TMP_Text maxAudioTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void initialise(AnnotationJsonData annotationData)
    {
        base.initialise(annotationData);
        if(annotationData is VoiceAnnotationJsonData voiceMessage)
        {
            audioPath = voiceMessage.AudioPath;
            //attempt to load audio
            StartCoroutine(loadAudio(audioPath));
        }
    }

    private IEnumerator loadAudio(string path)
    {
        string audioFilePath = $"file://{path}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioFilePath, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                //get the audio clip from the handler
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                //set the audio source clip
                audioSource.clip = clip;
                //initialise audio player
                initialiseAudioControls();
            }
            else
            {
                Debug.LogError($"Failed to load audio clip using path {audioFilePath}\nerror: {www.error}");
            }
        }
    }

    private void initialiseAudioControls()
    {
        //add toggle playback as the event callback
        playButton.OnClicked.AddListener(togglePlayback);
        //set the max value of the seek slider to length of the audio
        seekSlider.MaxValue = audioSource.clip.length;
        //add on value changed callback
        seekSlider.OnValueUpdated.AddListener(onSeekSliderValueChanged);
        //set the current time to the time of the audio
        currentAudioTime.text = audioSource.time.ToString();
        //set the max audio time to the length of the audio source
        maxAudioTime.text = audioSource.clip.length.ToString();
    }

    private void onSeekSliderValueChanged(SliderEventData seekData)
    {
        if(!audioSource.clip)
        {
            Debug.LogError($"Cannot adjust seek of {transform.name} slider");
            return;
        }
        //seek the audio time to the sliders current value
        audioSource.time = seekData.NewValue;
        //ensure that the audio doesnt randomly start playing when seeking
        if(!isPlaying)
        {
            audioSource.Play();
            audioSource.Pause();
        }
    }

    private void togglePlayback()
    {
        isPlaying = !isPlaying;
        if(isPlaying)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void Update()
    {
        if(isPlaying)
        {
            seekSlider.Value = audioSource.time;
            currentAudioTime.text = audioSource.time.ToString();
        }
    }

}
