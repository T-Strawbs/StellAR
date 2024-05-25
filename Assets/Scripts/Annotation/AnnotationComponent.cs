using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.AnnotationUtility;


public class AnnotationComponent : MonoBehaviour
{
    public int annotationCount = 0;
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
    public Color originalColour;


    public List<AnnotationJson> Annotations
    {
        get { return annotations; }
        set { annotations = value; }
    }

    // when this compojnent is added to a GameObject set the original colour value
    private void Awake()
    {
        Renderer renderer = this.GetComponent<Renderer>();
        if (renderer != null)
        {
            this.originalColour = renderer.material.color;
            setHighlight(this.highlightColour);
        }
    }

    // modify renderer material colour and the private highlightColour identifier string
    public void setHighlight(string newHighlight)
    {
        if (this.GetComponent<Renderer>() != null)
        {
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
}