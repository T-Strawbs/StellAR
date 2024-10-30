using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;


public class AnnotationComponent : MonoBehaviour
{
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

    // set when created by script
    public string originalColourString { get; private set; }

    public List<AnnotationJson> Annotations
    {
        get { return annotations; }
        set { annotations = value; }
    }

    private void Start()
    {
        setHighlight(highlightColour);
    }

    // called when new AnnotationComponent is added to object, sets originalColourString and renderer material colour
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


    // modify renderer material colour and the private highlightColour identifier string
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

    // public facing function for updating the highlight colour of this object and its children
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

        // update the Json
        AnnotationManager.Instance.updateAnnotationHighlightJson(newHighlightColour);
    }

    [Rpc(SendTo.Server)]
    public void changeHighlightColourServerRpc(string newHighlightColour)
    {

    }

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