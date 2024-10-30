using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public enum ButtonState
{
    START,
    STOP,
    DELETE
}

public class VoiceInput : MonoBehaviour, IAnnotationInput
{
    /// <summary>
    /// The record btn
    /// </summary>
    [SerializeField] private PressableButton recordBtn;
    [SerializeField] private FontIconSelector recordBtnIcon;
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
        postBtn.OnClicked.AddListener(postAnnotation);
    }

    private void OnRecordButtonClicked()
    {
        if(recordBtnState == ButtonState.START)
        {
            recordAudio();
            recordBtnIcon.CurrentIconName = "Icon 135";
            //icon 135 = audio stop buttno
        }
        else if(recordBtnState == ButtonState.STOP)
        {
            recordAudio();
            recordBtnIcon.CurrentIconName = "Icon 80";
            //icon 80 is remove icon
        }
        else
        {
            //reset the input 
            resetVoiceInput();
        }
    }

    private void recordAudio()
    {
        string defaultDeviceName = Microphone.devices[0];
        DebugConsole.Instance.LogDebug($"Recording from mic {defaultDeviceName}");

        if (string.IsNullOrEmpty(defaultDeviceName))
        {
            DebugConsole.Instance.LogError($"we cant start or stop recording because we couldnt find a mic");
            return;
        }
        if (!isRecording)
        {
            isRecording = true;
            //start recording using the the default microphone
            currentRecording = Microphone.Start
                (
                    defaultDeviceName, false, GlobalConstants.RECORDING_MAX_DURATION, 44100
                );
            //capture recording start time so we can trim the track once we end recording
            recordingStartTime = Time.time;
            //set the button state to Stop so the next tap will stop the recording
            recordBtnState = ButtonState.STOP;
            return;
        }
        isRecording = false;
        //stop the recording 
        Microphone.End(defaultDeviceName);
        //set the button state to Delete so the next tap will delete the curent recording
        recordBtnState = ButtonState.DELETE;
        //check if we actually recorded somthing
        if (!currentRecording)
        {
            DebugConsole.Instance.LogError("We didnt record anything just a heads up");
            return;
        }
        //calc the recording duration
        float recordingDuration = Time.time - recordingStartTime;
        //trim the audio so that audio is only as long as the recording was
        trimAudioClip(recordingDuration);
        //set the audioplayers clip to the current recording
        audioPlayerUI.setAudioSource(currentRecording);
        
    }

    private void trimAudioClip(float recordingDuration)
    {
        if (!currentRecording)
        {
            DebugConsole.Instance.LogError("current recording is null so we cant trim it");
        }


        //create a float array to hold the samples
        float[] currentSamples = new float[currentRecording.samples];
        //populate the array with samples from the current recording
        currentRecording.GetData(currentSamples, 0);
        //set the 
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

    public void postAnnotation()
    {
        if(ApplicationManager.Instance.isOnline())
        {
            postOnline();
        }
        else
        {
            postLocally();
        }

    }

    private void postLocally()
    {
        //check if theres a currently selected object
        if (!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError("We cant post when we have no currently selected object");
            return;
        }
        //quickly assert that the current selection has an annotation component
        AnnotationComponent annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("We cant post as the currently selected object has no annotation component");
            return;
        }
        //check if we're recording
        if (isRecording)
        {
            DebugConsole.Instance.LogError("We cant post while we are recording");
            return;
        }
        //check if we have a clip to post
        if (currentRecording == null)
        {
            DebugConsole.Instance.LogError("We cant post as there is no current recording");
            return;
        }
        //get the current date and time to store in the annotation data
        string currentDateTime = DateTime.Now.ToString(GlobalConstants.TIME_FORMAT);
        //format the current datetime so that we can save a file without IO pointing a gun at us
        string dateTimeFormatted = currentDateTime.Replace(':', '-').Replace(' ', '-').Replace('/', '-');
        //create filename from the componet name + datetime
        string fileName = $"{SelectionManager.Instance.currentSelection.name}_{"DefaultAuthor"}_{dateTimeFormatted}";
        //save audio to file
        SavWav.Save(fileName, currentRecording);
        //tell Annotation manager to create annotation Json
        AnnotationManager.Instance.createAnnotationJson(
            SelectionManager.Instance.currentSelection.transform,
            GlobalConstants.VOICE_ANNOTATION,
            "Default Author",// we need to replace this once we have multiple active users
            currentDateTime,
            $"{GlobalConstants.ANNOTATION_DIR}/{fileName}.wav"
            );
        //tell the UI manager to update its annotations 
        DataPanelManager.Instance.updateAnnotations(annotationComponent);
        DebugConsole.Instance.LogDebug("we wouldve \"created\" a voice annotation");
        //reset content
        resetVoiceInput();
    }

    public void resetVoiceInput()
    {
        //remove the current recording
        currentRecording = null;
        //clear the audio player
        audioPlayerUI.clear();
        //set the button state to Delete so the next tap will delete the curent recording
        recordBtnState = ButtonState.START;
        recordBtnIcon.CurrentIconName = "Icon 128";
    }

    private void postOnline()
    {
        //check if there's a currently selected object
        if (!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError("We can't post when we have no currently selected object");
            return;
        }

        //quickly assert that the current selection has an annotation component
        AnnotationComponent annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("We can't post as the currently selected object has no annotation component");
            return;
        }

        //check if we're recording
        if (isRecording)
        {
            DebugConsole.Instance.LogError("We can't post while we are recording");
            return;
        }

        //check if we have a clip to post
        if (currentRecording == null)
        {
            DebugConsole.Instance.LogError("We can't post as there is no current recording");
            return;
        }

        //convert AudioClip into float array, from Unity scripting docs for AudioClip.GetData
        var numSamples = currentRecording.samples * currentRecording.channels;
        float[] samples = new float[numSamples];
        currentRecording.GetData(samples, 0);        

        MessageBasedInteractable addAnnotationToThis = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();
        AnnotationManager.Instance.postAudioAnnotationServerRpc(addAnnotationToThis.lookupData, samples, numSamples, currentRecording.channels, currentRecording.frequency);

        //reset content
        resetVoiceInput();
    }
}

