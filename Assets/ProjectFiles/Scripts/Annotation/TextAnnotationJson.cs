using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Serialised concrete class to hold Json Objects in the form of Text Annotations.
/// </summary>
[Serializable]
public class TextAnnotationJson : AnnotationJson
{
    /// <summary>
    /// The format type of this annotation object which in this case should always be "Text"
    /// </summary>
    public override string MessageType => GlobalConstants.TEXT_ANNOTATION;

    /// <summary>
    /// Constructor for this Text Annotation Object
    /// </summary>
    /// <param name="componentName"></param>
    /// <param name="author"></param>
    /// <param name="timestamp"></param>
    /// <param name="content"></param>
    [JsonConstructorAttribute]
    public TextAnnotationJson(string componentName, string author, string timestamp, string content)
    {
        ComponentName = componentName;
        Author = author;
        Timestamp = timestamp;
        Content = content;
    }

    /// <summary>
    /// Convert network annotation to Text Annotation
    /// </summary>
    /// <param name="networkAnnotation">Network annotation passed over network via Rpc</param>
    public TextAnnotationJson(NetworkAnnotationJson networkAnnotation)
    {
        ComponentName = networkAnnotation.ComponentName;
        Author = networkAnnotation.Author;
        Timestamp = networkAnnotation.Timestamp;
        Content = networkAnnotation.Content;
    }
}
