using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public enum AudioState { PLAYING,PAUSED,STOPPED}

[RequireComponent(typeof(AudioSource))]
public class AudioPlayerUI : MonoBehaviour
{
    public string audioPath { get; set; }

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private PressableButton playButton;
    [SerializeField] private Slider seekSlider;

    [SerializeField] private AudioState audioState = AudioState.STOPPED;

    [SerializeField] private TMP_Text currentAudioTime;
    [SerializeField] private TMP_Text maxAudioTime;
    [SerializeField] private FontIconSelector icon;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        icon = GetComponentInChildren<FontIconSelector>();
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
        if(clip != null)
        {
            initialiseAudioControls();
            DebugConsole.Instance.LogDebug("Received audio data from server and set UI correctly.");
        }
        else
        {
            DebugConsole.Instance.LogDebug("Received null audio data from server.");
        }
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
        if(audioState != AudioState.PLAYING)
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
            audioState = AudioState.STOPPED;
        }
    }

    private void togglePlayback()
    {
        // if online and not hosting
        if (ApplicationManager.Instance.isOnline() && !NetworkManager.Singleton.IsHost)
        {
            if (audioSource.clip == null)
            {
                MessageBasedInteractable currentSelection = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();
                if (currentSelection != null)
                {
                    //load audio data into audiosource.clip
                    AudioLoader.Instance.requestAudioFromServer(audioPath, this, currentSelection.lookupData);
                    StartCoroutine(awaitRemoteAudioLoad());
                }
                else
                {
                    DebugConsole.Instance.LogError("Tried to post voice annotation but current selection is not networked.");
                }
            }
        }

        //check if we have an audio clip, only for offline as when online the clip will play after being loaded.
        if(audioSource.clip == null)
        {
            DebugConsole.Instance.LogError("Cant play audio as there is no clip.");
            return;
        }
        if(audioState != AudioState.PLAYING)//if we're not playing audio
        {
            playAudio();
        }
        else//we're playing and we've pressed the play button so we should pause
        {
            pauseAudio();
        }
    }

    /// <summary>
    /// Asynchronous function that checks if the remote loading of audio clip is successful or not.
    /// </summary>
    /// <returns></returns>
    private IEnumerator awaitRemoteAudioLoad()
    {
        //Disable play button until request times out or clip is successfully loaded
        playButton.enabled = false;
        playButton.GetComponentInChildren<FontIconSelector>().CurrentIconName = "Icon 3";

        // wait until clip is loaded or max wait time of x seconds (x = MAX_WAIT_TIME)
        float timeoutDuration = 0f;
        while (timeoutDuration < GlobalConstants.MAX_WAIT_TIME && audioSource.clip == null)
        {
            yield return new WaitForEndOfFrame();
            timeoutDuration += Time.deltaTime;
        }

        playButton.enabled = true;
        playButton.GetComponentInChildren<FontIconSelector>().CurrentIconName = "Icon 122";

        // if max wait time was exceeded i.e. clip wasn't able to be loaded
        if (timeoutDuration >= GlobalConstants.MAX_WAIT_TIME)
        {
            DebugConsole.Instance.LogError("Unable to load audio annotation from server.");
            yield return null;
        }
        else
        {
            DebugConsole.Instance.LogError($"APUI_awaitRemoteAudioLoad: we should have audio? audioclip = {audioSource.clip != null}");
            playAudio();
        }
    }

    private void Update()
    {
        if(audioState == AudioState.PLAYING)
        {
            //move slider to track time
            seekSlider.Value = audioSource.time;
            //update the current time text element to show mm::ss
            updateTimeText(audioSource.time,currentAudioTime);
        }
        
        if(!audioSource.isPlaying && audioState != AudioState.PAUSED)
        {
            stopAudio();
        }
    }

    private void updateTimeText(float trackTime,TMP_Text textUI)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(trackTime);
        textUI.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    private void playAudio()
    {
        //set the  current state of the audio to playing
        audioState = AudioState.PLAYING;
        audioSource.Play();
        icon.CurrentIconName = "Icon 46";
    }

    private void pauseAudio()
    {
        //set the current audio state to paused
        audioState = AudioState.PAUSED;
        audioSource.Pause();
        icon.CurrentIconName = "Icon 122";
    }

    private void stopAudio()
    {
        audioState = AudioState.STOPPED;
        audioSource.Stop();
        audioSource.time = 0;
        seekSlider.Value = 0;
        updateTimeText(0f, currentAudioTime);
        icon.CurrentIconName = "Icon 122";
    }

    public void clear()
    {
        //check if we're playing
        if(audioSource.isPlaying)
        {
            //stop audio
            audioSource.Pause();
        }
        //remove clip from audio source
        audioSource.clip = null;
        //reset seek slider
        seekSlider.Value = 0;
        //reset time texts
        currentAudioTime.text = "0:00";
        maxAudioTime.text = "0:00";
    }

}
