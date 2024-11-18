using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - UNisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class that represents the UI pane for displaying the metadata of our selected object
/// </summary>
public class MetadataPane : MonoBehaviour
{
    /// <summary>
    /// The text object for displaying the metadata content
    /// </summary>
    [SerializeField] private TMP_Text metadataContent;

    /// <summary>
    /// method for updating the content of the metadata pane's text object
    /// </summary>
    /// <param name="metadata"></param>
    public void updateMetadataContent(string metadata)
    {
       metadataContent.text = metadata;
    }
}
