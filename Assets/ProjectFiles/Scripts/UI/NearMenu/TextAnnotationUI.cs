using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// concrete class for displayi
/// </summary>
public class TextAnnotationUI : AnnotationUI
{
    /// <summary>
    /// Text UI element that holds the annotation content
    /// </summary>
    [SerializeField] private TMP_Text content;
    /// <summary>
    /// The transform of this this AnnotationUI
    /// </summary>
    [SerializeField] private RectTransform textAnnotationRect;
    /// <summary>
    /// The transform of this annotation's center layer, we only have this here so
    /// we can drag a reference from editor to this object to handle dynamic resizing
    /// of this UI element.
    /// </summary>
    [SerializeField] private RectTransform centreLayer;
    /// <summary>
    /// The transform of this annotation's content layer, we only have this here so
    /// we can drag a reference from editor to this object to handle dynamic resizing
    /// of this UI element.
    /// </summary>
    [SerializeField] private RectTransform contentLayer;

    public override void initialise(AnnotationJson annotationData)
    {
        base.initialise(annotationData);
        if(annotationData is TextAnnotationJson textMessage)
        {
            content.text = textMessage.Content;
        }
        else
        {
            DebugConsole.Instance.LogError($"Invalid message type parsed to {transform.name}");
        }
        resizeContentLayer();
    }

    /// <summary>
    /// Method for resizing this Annotation UI element after we initialise it.
    /// </summary>
    private void resizeContentLayer()
    {
        //force the animationName text to resize based on the animationName length
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.rectTransform);
        //create a vector to store the same dimentions as the animationName text
        Vector2 contentSize = new Vector2(content.rectTransform.rect.width, content.rectTransform.rect.height);
        //resize contentlayer
        contentLayer.sizeDelta = contentSize;
        //
        RectTransform btnRect = deleteBtn.GetComponent<RectTransform>();
        if(!btnRect)
        {
            DebugConsole.Instance.LogError("RectTransform of deleteBtn Couldnt be found");
        }
        else
        {
            //calculate the new dimensions for centrelayer
            Vector2 centreSize = new Vector2(btnRect.rect.width + contentSize.x, contentSize.y);
            //change the height of centrelayer (contentSizes parent)
            centreLayer.sizeDelta = centreSize;
            //force the animationName text to resize based on the animationName length
            LayoutRebuilder.ForceRebuildLayoutImmediate(textAnnotationRect);
        }
        

    }

}
