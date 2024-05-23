using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayerUI : MonoBehaviour
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
    private void Start()
    {
        seekSlider.Value = 0;
        //add toggle playback as the event callback
        playButton.OnClicked.AddListener(togglePlayback);
        //add on value changed callback
        seekSlider.OnValueUpdated.AddListener(onSeekSliderValueChanged);
    }

    public void setAudioSource(AudioClip clip)
    {
        audioSource.clip = clip;
        initialiseAudioControls();
    }

    private void initialiseAudioControls()
    {
        //set the max value of the seek slider to length of the audio
        seekSlider.MaxValue = audioSource.clip.length;
        //set the current time to the time of the audio
        updateTimeText(0f, currentAudioTime);
        //set the max audio time to the length of the audio source
        updateTimeText(audioSource.clip.length,maxAudioTime);
    }

    private void onSeekSliderValueChanged(SliderEventData seekData)
    {
        if(!audioSource.clip)
        {
            DebugConsole.Instance.LogError($"Cannot adjust seek of {transform.name} slider");
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

    private void OnEnable()
    {
        if(audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.time = 0f;
            seekSlider.Value = 0f;
        }
    }

    private void togglePlayback()
    {
        //check if we have an audio clip
        if(audioSource.clip == null)
        {
            DebugConsole.Instance.LogError("Cant play audio as there is no clip.");
            return;
        }
        if(!isPlaying)
        {
            isPlaying = true;
            audioSource.Play();
            return;
        }
        isPlaying = false;
        audioSource.Pause();
    }

    private void Update()
    {
        if(isPlaying)
        {
            //move slider to track time
            seekSlider.Value = audioSource.time;
            //update the current time text element to show mm::ss
            updateTimeText(audioSource.time,currentAudioTime);
        }
    }

    private void updateTimeText(float trackTime,TMP_Text textUI)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(trackTime);
        textUI.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

}
