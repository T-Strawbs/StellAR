using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ButtonState
{
    START,
    STOP,
    DELETE
}
public class VoiceInput : MonoBehaviour
{
    /// <summary>
    /// The record btn
    /// </summary>
    [SerializeField] private PressableButton recordBtn;
    /// <summary>
    /// The current state of the record button (START,STOP,DELETE)
    /// </summary>
    [SerializeField] private ButtonState recordBtnState;

    /// <summary>
    /// The button that posts the recording
    /// </summary>
    [SerializeField] private PressableButton postBtn;
    /// <summary>
    /// The audio player UI component
    /// </summary>
    [SerializeField] private AudioPlayerUI audioPlayerUI;

    [SerializeField] private bool isRecording;
    [SerializeField] private AudioClip currentRecording;

    [SerializeField] private float recordingStartTime = 0f;

    private void Start()
    {
        isRecording = false;
        recordBtnState = ButtonState.START;
        //setup event listener callback for the recordbutton
        recordBtn.OnClicked.AddListener(OnRecordButtonClicked);
        //setup event call back for clicking the post button
        postBtn.OnClicked.AddListener(postVoiceAnnotation);
    }

    private void OnRecordButtonClicked()
    {
        if(recordBtnState == ButtonState.START)
        {
            recordAudio();
        }
        else if(recordBtnState == ButtonState.STOP)
        {
            recordAudio();
        }
        else
        {
            //remove the current recording
            currentRecording = null;
            //set the button state to Delete so the next tap will delete the curent recording
            recordBtnState = ButtonState.START;
        }
    }

    private void recordAudio()
    {
        string defaultDeviceName = Microphone.devices[0];
        if (defaultDeviceName == "")
        {
            Debug.LogError($"we cant start or stop recording because we couldnt find a mic");
            return;
        }
        if (!isRecording)
        {
            isRecording = true;
            //start recording using the the default microphone
            currentRecording = Microphone.Start(defaultDeviceName, false, 120, 44100);
            //capture recording start time so we can trim the track once we end recording
            recordingStartTime = Time.time;
            //set the button state to Stop so the next tap will stop the recording
            recordBtnState = ButtonState.STOP;
            return;
        }
        isRecording = false;
        //stop the recording 
        Microphone.End(defaultDeviceName);
        //calc the recording duration
        float recordingDuration = Time.time - recordingStartTime;
        //trim the audio so that audio is only as long as the recording was
        trimAudioClip(recordingDuration);
        //set the audioplayers clip to the current recording
        audioPlayerUI.setAudioSource(currentRecording);
        //set the button state to Delete so the next tap will delete the curent recording
        recordBtnState = ButtonState.STOP;
    }

    private void trimAudioClip(float recordingDuration)
    {
        //create a float array to hold the samples
        float[] currentSamples = new float[currentRecording.samples];
        //populate the array with samples from the current recording
        currentRecording.GetData(currentSamples, 0);
        //create an array to store the trimmed samples with a length of the current samples over the recording 
        //duration multiplied by the audio frequency and the number of channels. TLDR its black magic.
        float[] trimmedSamples = new float[
            (int)(recordingDuration * currentRecording.frequency * currentRecording.channels)];
        //copy the current samples over to the trimmed sample [] to only include the samples up until the
        //recording stop time
        Array.Copy(currentSamples, trimmedSamples, trimmedSamples.Length);
        //create a new clip from the current recording specs now at the proper duration.
        AudioClip trimmedClip = AudioClip.Create
            (currentRecording.name, trimmedSamples.Length,
            currentRecording.channels, currentRecording.frequency,
            false);
        //set the data for the new clip
        trimmedClip.SetData(trimmedSamples, 0);
        currentRecording = trimmedClip;
    }

    private void postVoiceAnnotation()
    {
        //check if we're recording
        if(isRecording)
        {
            Debug.Log("We cant post while we are recording");
            return;
        }
        //check if we have a clip to post
        if(currentRecording == null)
        {
            Debug.Log("We cant post as there is no current recording");
            return;
        }
        //post to UI manager to create annotation

        //post to annotation manager to create a AnnotationComponent to add to the
        //currently selected component

        //write new annotation to JSON 

    }





}
