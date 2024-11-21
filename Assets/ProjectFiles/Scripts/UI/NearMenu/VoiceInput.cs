using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class that facilitates and handles the recording of voice annotations.
/// </summary>
public class VoiceInput : MonoBehaviour, IAnnotationInput
{
    /// <summary>
    /// The record btn
    /// </summary>
    [SerializeField] private PressableButton recordBtn;
    /// <summary>
    /// The icon that is displayed on the record btn ui element
    /// </summary>
    [SerializeField] private FontIconSelector recordBtnIcon;
    /// <summary>
    /// The current state of the record button (START,STOP,DELETE)
    /// </summary>
    [SerializeField] private ButtonState recordBtnState;
    /// <summary>
    /// The button that begins the posting process of the voice annotation
    /// </summary>
    [SerializeField] private PressableButton postBtn;
    /// <summary>
    /// The audio player UI component
    /// </summary>
    [SerializeField] private AudioPlayerUI audioPlayerUI;
    /// <summary>
    /// Boolean for determining whether we are recording or not
    /// </summary>
    [SerializeField] private bool isRecording;
    /// <summary>
    /// the audioclip that we will record to
    /// </summary>
    [SerializeField] private AudioClip currentRecording;
    /// <summary>
    /// the start time of the recording
    /// </summary>
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

    /// <summary>
    /// callback for cycling through the record button states when
    /// clicked.
    /// </summary>
    private void OnRecordButtonClicked()
    {
        if(recordBtnState == ButtonState.START)
        {
            recordAudio();
            //icon 135 = audio stop buttpn
            recordBtnIcon.CurrentIconName = "Icon 135";
            
        }
        else if(recordBtnState == ButtonState.STOP)
        {
            recordAudio();
            //icon 80 is remove icon
            recordBtnIcon.CurrentIconName = "Icon 80";
            
        }
        else
        {
            //reset the input 
            resetVoiceInput();
        }
    }
    /// <summary>
    /// callback method for recording audio
    /// </summary>
    private void recordAudio()
    {
        //get the name of the first microphone detected -- At some point it would be better for
        //the user to choose the device but at the moment we're just a proof of concept.
        string defaultDeviceName = Microphone.devices[0];
        DebugConsole.Instance.LogDebug($"Recording from mic {defaultDeviceName}");
        //check if the device exists, if not return
        if (string.IsNullOrEmpty(defaultDeviceName))
        {
            DebugConsole.Instance.LogError($"we cant start or stop recording because we couldnt find a mic");
            return;
        }
        //check if we are currently recording
        if (!isRecording)
        {
            //we aren't so we begin the recording process.
            isRecording = true;
            //start recording using the the default microphone
            currentRecording = Microphone.Start
                (
                    defaultDeviceName, false, GlobalConstants.RECORDING_MAX_DURATION, GlobalConstants.SAMPLE_RATE
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="recordingDuration"></param>
    private void trimAudioClip(float recordingDuration)
    {
        //check if we have a current recording
        if (!currentRecording)
        {
            //we don't so return
            DebugConsole.Instance.LogError("current recording is null so we cant trim it");
            return;
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
        //check if the application is currently online
        if(ApplicationManager.Instance.isOnline())
        {
            postOnline();
        }
        else
        {
            postLocally();
        }

    }
    /// <summary>
    /// Method for posting a voice annotation locally
    /// </summary>
    /// <returns>AnnotationJson: The created annotation data</returns>
    private AnnotationJson postLocally()
    {
        //check if theres a currently selected object
        if (!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError("We cant post when we have no currently selected object");
            return null;
        }
        //quickly assert that the current selection has an annotation component
        AnnotationList annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationList>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("We cant post as the currently selected object has no annotation component");
            return null;
        }
        //check if we're recording
        if (isRecording)
        {
            DebugConsole.Instance.LogError("We cant post while we are recording");
            return null;
        }
        //check if we have a clip to post
        if (currentRecording == null)
        {
            DebugConsole.Instance.LogError("We cant post as there is no current recording");
            return null;
        }

        //get the current date and time to store in the annotation data
        string currentDateTime = DateTime.Now.ToString(GlobalConstants.TIME_FORMAT);
        //format the current datetime so that we can save a file without IO pointing a gun at us
        string dateTimeFormatted = currentDateTime.Replace(':', '-').Replace(' ', '-').Replace('/', '-');
        //create filename from the componet name + datetime
        string fileName = $"{GlobalConstants.ANNOTATION_DIR}/{SelectionManager.Instance.currentSelection.name}/{SelectionManager.Instance.currentSelection.name}_{"DefaultAuthor"}_{dateTimeFormatted}";
        //save audio to file
        SavWav.Save(fileName, currentRecording);
        //tell Annotation manager to create annotation Json
        AnnotationJson audioAnnotation = AnnotationManager.Instance.createAnnotationJson(
            SelectionManager.Instance.currentSelection.transform,
            GlobalConstants.VOICE_ANNOTATION,
            "Default Author",// we need to replace this once we have multiple active users
            currentDateTime,
            $"{fileName}.wav"
            );
        //tell the UI manager to update its annotations 
        DataPanelManager.Instance.updateAnnotations(annotationComponent);
        DebugConsole.Instance.LogDebug("we wouldve \"created\" a voice annotation");
        //reset animationName
        resetVoiceInput();
        return audioAnnotation;
    }

    /// <summary>
    /// callback for reseting the voice input
    /// </summary>
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
    /// <summary>
    /// method for posting the voice annoation over the network to the host.
    /// </summary>
    private void postOnline()
    {
        //check if there's a currently selected object
        if (!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError("We can't post when we have no currently selected object");
            return;
        }

        //quickly assert that the current selection has an annotation component
        AnnotationList annotationComponent = SelectionManager.Instance.currentSelection.GetComponent<AnnotationList>();
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

        if(!NetworkManager.Singleton.IsHost)
        {
            //convert AudioClip into float array, from Unity scripting docs for AudioClip.GetData
            var numSamples = currentRecording.samples * currentRecording.channels;
            float[] samples = new float[numSamples];
            currentRecording.GetData(samples, 0);

            MessageBasedInteractable addAnnotationToThis = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();
            if (addAnnotationToThis != null)
            {
                AnnotationManager.Instance.postAudioAnnotationServer(addAnnotationToThis.lookupData, samples, currentRecording.channels);
            }
            else
            {
                DebugConsole.Instance.LogError("Tried to post audio annotation while online but currently selected object is not networked (MessageBasedInteractable)");
            }
        }
        // if you are hosting, just post locally and broadcast the annotation created.
        else
        {
            AnnotationJson createdAnnotation = postLocally();
            MessageBasedInteractable addAnnotationToThis = SelectionManager.Instance.currentSelection.GetComponent<MessageBasedInteractable>();

            //broadcast new annotation to clients
            NetworkAnnotationJson networkAnnotation = new NetworkAnnotationJson(createdAnnotation);
            AnnotationManager.Instance.broadcastNewAnnotationRpc(addAnnotationToThis.lookupData, networkAnnotation);

            //if annotation was for current selection update data pane
            if (SelectionManager.Instance.currentSelection == addAnnotationToThis.GetComponent<Interactable>())
            {
                DataPanelManager.Instance.updateAnnotations(annotationComponent);
            }
        }

        //reset animationName
        resetVoiceInput();
    }
}
public enum ButtonState
{
    START,
    STOP,
    DELETE
}
