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
/// Class that represents the pane that displays annotation data to our users.
/// </summary>
public class AnnotationPane : MonoBehaviour
{
    /// <summary>
    /// List for storing references for all the actively displayed annotations ui elements on our pane.
    /// </summary>
    [SerializeField] private List<AnnotationUI> activeAnnotationUI = new List<AnnotationUI>();
    /// <summary>
    /// The tranform of the UI element that holds all our active annotation ui elements if any.
    /// </summary>
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
}
