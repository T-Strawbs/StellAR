using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationPane : MonoBehaviour
{
    [SerializeField] private List<AnnotationUI> activeAnnotationUI;
    [SerializeField] private RectTransform contentHolder;

    public List<AnnotationUI> ActiveAnnotationUI
    {
        get
        {
            return activeAnnotationUI;
        }
    }
    public RectTransform ContentHolder
    {
        get
        {
            return contentHolder;
        }
    }

    private void Awake()
    {
        activeAnnotationUI = new List<AnnotationUI>();
    }

    

    
}
