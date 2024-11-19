using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Game Component for fast access of annotation messages. Holds annotation data for the attached model component
/// to be modified by the user and to be read and writen to by the AnnotationManager.
/// </summary>
public class AnnotationComponent : MonoBehaviour
{
    /// <summary>
    /// The list of annotation messages from json data
    /// </summary>
    private List<AnnotationJson> annotations = new List<AnnotationJson>();

    public string highlightColour = "None";
    /*
    Valid string values:
        Green,
        Yellow,
        Red,
        Blue,
        None
    */

    /// <summary>
    /// The colour code string for keeping reference of the original colour code of this model component
    /// </summary>
    public string originalColourString { get; private set; }

    /// <summary>
    /// Property for the annotation data list.
    /// </summary>
    public List<AnnotationJson> Annotations
    {
        get { return annotations; }
        set { annotations = value; }
    }

    private void Start()
    {
        setHighlight(highlightColour);
    }

    /// <summary>
    /// called when new AnnotationComponent is added to object, sets originalColourString and renderer material colour
    /// </summary>
    /// <returns></returns>
    public string getOriginalolourString()
    {
        // if original colour code has already been set don't set it
        if (originalColourString == null)
        {
            Renderer renderer = this.GetComponent<Renderer>();
            if (renderer != null)
            {
                this.originalColourString = "#" + ColorUtility.ToHtmlStringRGBA(renderer.material.color);
            }
        }
        return originalColourString;
    }


    /// <summary>
    /// modify renderer material colour and the private highlightColour identifier string
    /// </summary>
    /// <param name="newHighlight"></param>
    public void setHighlight(string newHighlight)
    {
        if (this.GetComponent<Renderer>() != null)
        {
            Color originalColour;
            ColorUtility.TryParseHtmlString(originalColourString, out originalColour);
            switch (newHighlight)
            {
                case "Green":
                    this.GetComponent<Renderer>().material.color = Color.Lerp(originalColour, Color.green, .5f);
                    this.highlightColour = newHighlight;
                    break;
                case "Yellow":
                    this.GetComponent<Renderer>().material.color = Color.Lerp(originalColour, Color.yellow, .5f);
                    this.highlightColour = newHighlight;
                    break;
                case "Red":
                    this.GetComponent<Renderer>().material.color = Color.Lerp(originalColour, Color.red, .5f);
                    this.highlightColour = newHighlight;
                    break;
                case "Blue":
                    this.GetComponent<Renderer>().material.color = Color.Lerp(originalColour, Color.blue, .5f);
                    this.highlightColour = newHighlight;
                    break;
                case "None":
                    this.GetComponent<Renderer>().material.color = originalColour;
                    this.highlightColour = newHighlight;
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// public facing function for updating the highlight colour of this object and its children
    /// </summary>
    /// <param name="newHighlightColour"></param>
    public void changeHighlightColour(string newHighlightColour)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (this.GetComponent<Renderer>() != null)
        {
            this.setHighlight(newHighlightColour);
        }

        // loop through
        foreach (Renderer renderer in renderers)
        {
            renderer.gameObject.GetComponent<AnnotationComponent>().setHighlight(newHighlightColour);
        }
    }
    /// <summary>
    /// method for deleting a given annotation from the annotations list.
    /// </summary>
    /// <param name="annotationData"></param>
    public void deleteAnnotation(AnnotationJson annotationData)
    {
        for(int i = 0; i < annotations.Count; i++)
        {
            if (annotations[i].Equals(annotationData))
            {
                annotations.RemoveAt(i);
                return;
            }
        }
    }

}