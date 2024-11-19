using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class representing the audio player UI element. 
/// Displays a play button, seekbar and audio playback info.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioPlayerUI : MonoBehaviour
{
    /// <summary>
    /// The path for the audio that this player is holding
    /// </summary>
    public string audioPath { get; set; }
    /// <summary>
    /// the audio source for facilitating playback
    /// </summary>
    [SerializeField] private AudioSource audioSource;
    /// <summary>
    /// UI element for playing the audio clip
    /// </summary>
    [SerializeField] private PressableButton playButton;
    /// <summary>
    /// UI element for controlling the playback position
    /// </summary>
    [SerializeField] private Slider seekSlider;
    /// <summary>
    /// the current state of the audio
    /// </summary>
    [SerializeField] private AudioState audioState = AudioState.STOPPED;
    /// <summary>
    /// the current playback time of the audio
    /// </summary>
    [SerializeField] private TMP_Text currentAudioTime;
    /// <summary>
    /// the end position of the audio
    /// </summary>
    [SerializeField] private TMP_Text maxAudioTime;
    /// <summary>
    /// the icon for the play button
    /// </summary>
    [SerializeField] private FontIconSelector playBtnIcon;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playBtnIcon = GetComponentInChildren<FontIconSelector>();
    }
    private void Start()
    {
        seekSlider.Value = 0;
        //add toggle playback as the event callback
        playButton.OnClicked.AddListener(togglePlayback);
        //add on value changed callback
        seekSlider.OnValueUpdated.AddListener(onSeekSliderValueChanged);
    }
    /// <summary>
    /// method for setting the audio clip for our audioplayer to play
    /// </summary>
    /// <param name="clip"></param>
    public void setAudioSource(AudioClip clip)
    {
        //set the audio source's clip to the new one
        audioSource.clip = clip;
        //check if the clip exists
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
    /// <summary>
    /// initialiser method for setting up the audio controls 
    /// </summary>
    private void initialiseAudioControls()
    {
        //set the max value of the seek slider to length of the audio
        seekSlider.MaxValue = audioSource.clip.length;
        //set the current time to the time of the audio
        updateTimeText(0f, currentAudioTime);
        //set the max audio time to the length of the audio source
        updateTimeText(audioSource.clip.length,maxAudioTime);
    }

    /// <summary>
    /// event listener method for updating the playback position based on the
    /// slider value of the seekbar.
    /// </summary>
    /// <param name="seekData"></param>
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

    /// <summary>
    /// Method for ensuring that when this UI element is enabled that the audio
    /// doesn't start playing without input from the user.
    /// </summary>
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

    /// <summary>
    /// callback method for handing the state of the audio playback.
    /// </summary>
    private void togglePlayback()
    {
        // if we're online and we  aren't the host
        if (ApplicationManager.Instance.isOnline() && !NetworkManager.Singleton.IsHost)
        {
            //check if the audio clip exists
            if (audioSource.clip == null)
            {
                //Grab the current selection from the selection manager
                MessageBasedInteractable currentSelection = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();
                //check that we actually have a current selection
                if (currentSelection != null)
                {
                    //Request the server to give us the audio data so we can add it into our audiosource.clip
                    AudioLoader.Instance.requestAudioFromServer(audioPath, this, currentSelection.lookupData);
                    //wait for a response from the server
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
        //check if our audiosource is currently playing
        if(audioState == AudioState.PLAYING)
        {
            //update the slider position to match the playback time.
            seekSlider.Value = audioSource.time;
            //update the current time text element to show mm::ss
            updateTimeText(audioSource.time,currentAudioTime);
        }
        //check if the audiosource is not playing and that the current state of playback is not paused
        if(!audioSource.isPlaying && audioState != AudioState.PAUSED)
        {
            //we want to stop the audiosource and handle the stop process of the audio player UI
            stopAudio();
        }
    }

    /// <summary>
    /// method for updating the current time text element to the current playback time.
    /// </summary>
    /// <param name="trackTime"></param>
    /// <param name="textUI"></param>
    private void updateTimeText(float trackTime,TMP_Text textUI)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(trackTime);
        textUI.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    /// <summary>
    /// method for starting audio playback 
    /// </summary>
    private void playAudio()
    {
        //set the  current state of the audio to playing
        audioState = AudioState.PLAYING;
        audioSource.Play();
        playBtnIcon.CurrentIconName = "Icon 46";
    }
    /// <summary>
    /// method for pausing the audio playback
    /// </summary>
    private void pauseAudio()
    {
        //set the current audio state to paused
        audioState = AudioState.PAUSED;
        audioSource.Pause();
        playBtnIcon.CurrentIconName = "Icon 122";
    }
    /// <summary>
    /// method for stopping the audio playback
    /// </summary>
    private void stopAudio()
    {
        audioState = AudioState.STOPPED;
        audioSource.Stop();
        audioSource.time = 0;
        seekSlider.Value = 0;
        updateTimeText(0f, currentAudioTime);
        playBtnIcon.CurrentIconName = "Icon 122";
    }
    /// <summary>
    /// method for "resetting" the audio player
    /// </summary>
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

public enum AudioState { PLAYING, PAUSED, STOPPED }
