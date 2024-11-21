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
/// Interface for classes that create annotations using their own input methods.
/// </summary>
public interface IAnnotationInput 
{
    /// <summary>
    /// callback for posting annotations
    /// </summary>
    public void postAnnotation();
}
