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
/// Serialisable class for Json objects that represent the model tree with  the model component's metadata and each models subcomponent.
/// </summary>
[Serializable]
public class MetadataJson
{
    /// <summary>
    /// the name of the model component
    /// </summary>
    public string name;
    /// <summary>
    /// the metadata for the model component
    /// </summary>
    public string metadata;
    /// <summary>
    /// a list of all direct subcomponent of this model component
    /// </summary>
    public List<MetadataJson> subcomponents;

    public MetadataJson(string nameIn)
    {
        name = nameIn;
        metadata = "";
        subcomponents = new List<MetadataJson>();
    }
}
