using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextAnnotationUI : AnnotationUI
{
    [SerializeField] private TMP_Text content;
    [SerializeField] private RectTransform textAnnotationRect;
    [SerializeField] private RectTransform centreLayer;
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

    private void resizeContentLayer()
    {
        //force the content text to resize based on the content length
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.rectTransform);
        //create a vector to store the same dimentions as the content text
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
            //force the content text to resize based on the content length
            LayoutRebuilder.ForceRebuildLayoutImmediate(textAnnotationRect);
        }
        

    }

}
