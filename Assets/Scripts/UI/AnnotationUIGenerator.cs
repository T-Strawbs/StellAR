using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responisble for instaniating and managing Annotation UI
/// prefabs.
/// </summary>
public class AnnotationUIGenerator : Singleton<AnnotationUIGenerator>
{
    /// <summary>
    /// <summary>
    /// Object pool for TextAnnotationUI elements
    /// </summary>
    [SerializeField] private List<TextAnnotationUI> pooledTextUI;
    /// <summary>
    /// Object pool for VoiceAnnotationUI elements
    /// </summary>
    [SerializeField] private List<VoiceAnnotationUI> pooledVoiceUI;

    [SerializeField] TextAnnotationUI textAnnotationUIPrefab;
    [SerializeField] VoiceAnnotationUI voiceAnnotationUIPrefab;

    private void Start()
    {
        pooledTextUI = new List<TextAnnotationUI>();
        pooledVoiceUI = new List<VoiceAnnotationUI>();
    }

    public void returnAnnotationUI(AnnotationUI annotationUI)
    {
        if(!annotationUI)
        {
            DebugConsole.Instance.LogError("the annotation was null so we couldnt return it");
            return;
        }
        if(annotationUI is TextAnnotationUI textUI)
        {
            textUI.gameObject.SetActive(false);
            textUI.transform.SetParent(transform);
            pooledTextUI.Add(textUI);
        }
        else if(annotationUI is VoiceAnnotationUI voiceUI)
        {
            voiceUI.gameObject.SetActive(false);
            voiceUI.transform.SetParent(transform);
            pooledVoiceUI.Add(voiceUI);
        }
    }

    public AnnotationUI GetAnnotationUI(AnnotationJson annotationData)
    {
        //check the type of the annotationData
        if(annotationData.MessageType == "Text")
        {
            TextAnnotationUI textAnnotationUI = null;
            //check if we have any text annotation UI elements in our pool
            if (pooledTextUI.Count < 1)
            {
                //generate new text UI element
                textAnnotationUI = (TextAnnotationUI)generateAnnotationUI(annotationData.MessageType);
            }
            else
            {
                //get the element at the top of this list
                textAnnotationUI = pooledTextUI[pooledTextUI.Count - 1];
                //"poll" the top element of the list 
                pooledTextUI.RemoveAt(pooledTextUI.Count - 1);
            }
            //map annotation UI data
            textAnnotationUI.initialise(annotationData);
            //return the textAnnotation
            return textAnnotationUI;
        }
        else if(annotationData.MessageType == "Voice")
        {
            VoiceAnnotationUI voiceAnnotationUI = null;
            //check if we have any voice annotation UI elements in our pool
            if (pooledVoiceUI.Count < 1)
            {
                //generate new text UI element
                voiceAnnotationUI = (VoiceAnnotationUI)generateAnnotationUI(annotationData.MessageType);
            }
            else
            {
                //get the element at the top of this list
                voiceAnnotationUI = pooledVoiceUI[pooledVoiceUI.Count - 1];
                //"poll" the top element of the list 
                pooledVoiceUI.RemoveAt(pooledVoiceUI.Count - 1);
            }
            //map annotation UI data
            voiceAnnotationUI.initialise(annotationData);
            //return the voiceAnnotation
            return voiceAnnotationUI;
        }
        DebugConsole.Instance.LogError($"Cannot create annotation with type {annotationData.MessageType} " +
            $"by author {annotationData.Author} at timestamp");
        return null;// **** we should be returning a default object to correctly error handle potentially.
    }

    private AnnotationUI generateAnnotationUI(string type)
    {
        if (type == "Text")
        {
            return Instantiate<TextAnnotationUI>(textAnnotationUIPrefab);
        }
        else if (type == "Voice")
        {
            return Instantiate<VoiceAnnotationUI>(voiceAnnotationUIPrefab);
        }
        else return null;
    }

    

    
}
