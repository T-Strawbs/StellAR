using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

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
    /// <summary>
    /// The prefab to generate TextAnnotationUI Element instances from
    /// </summary>
    [SerializeField] TextAnnotationUI textAnnotationUIPrefab;
    /// <summary>
    /// The prefab to generate VoiceAnnotationUI Element instances from
    /// </summary>
    [SerializeField] VoiceAnnotationUI voiceAnnotationUIPrefab;

    private void Start()
    {
        pooledTextUI = new List<TextAnnotationUI>();
        pooledVoiceUI = new List<VoiceAnnotationUI>();
    }

    /// <summary>
    /// method for reclaiming the used annotation UI elements and deactivating them.
    /// </summary>
    /// <param name="annotationUI"></param>
    public void returnAnnotationUI(AnnotationUI annotationUI)
    {
        //check if the element is null and return if it is.
        if(!annotationUI)
        {
            DebugConsole.Instance.LogError("the annotation was null so we couldnt return it");
            return;
        }
        //check if the annotation element is either a text or voice annotation
        if(annotationUI is TextAnnotationUI textUI)
        {
            //reparent the element to this generator so that its out of the annotation pane's
            //animationName holder
            textUI.transform.SetParent(transform);
            //add the element to the pool of text ui elements
            pooledTextUI.Add(textUI);
            //deactivate the element
            textUI.gameObject.SetActive(false);
        }
        else if(annotationUI is VoiceAnnotationUI voiceUI)
        {
            //reparent the element to this generator so that its out of the annotation pane's
            //animationName holder
            voiceUI.transform.SetParent(transform);
            //add the element to the pool of text ui elements
            pooledVoiceUI.Add(voiceUI);
            //deactivate the element
            voiceUI.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// method for retrieving an instance of an annotation UI element based on the annotation data.
    /// </summary>
    /// <param name="annotationData"></param>
    /// <returns>AnnotationUI: the annotation element that would display the annotation data, 
    /// Null if theres an issue.</returns>
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

    /// <summary>
    /// Method for creating an instance of an AnnotationUI element based on the annotation type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
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
