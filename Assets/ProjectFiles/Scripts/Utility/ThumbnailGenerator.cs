using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class responisble for generating thumbnails of models at runtime. 
/// </summary>
public class ThumbnailGenerator : Singleton<ThumbnailGenerator>
{
    /// <summary>
    /// The desired width of the thumbnail image
    /// </summary>
    [SerializeField] private int thumbnailWidth = 32;
    /// <summary>
    /// The desired height of the thumbnail image
    /// </summary>
    [SerializeField] private int thumbnailHeight = 32;

    /// <summary>
    /// method for generating the thumbnail for a given prefab.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public Texture2D getModelThumbnail(GameObject prefab)
    {
        //The RuntimePreviewGenerator requires an instance of a prefab to generate a
        //thumbnail of a given model. This is annoying as we want the model menu UI to have 
        //a thumbnail of models before they are instantiated by the user so our solution is 
        //to generate an instance of the model prefab within a single frame and delete it.

        //We need to temporarily instantiate the prefab to work with the RuntimePreviewGenerator
        GameObject tempModel = Instantiate(prefab);
        //generate the thumbnail
        Texture2D modelThumbnail = RuntimePreviewGenerator.GenerateModelPreview
            (
                tempModel.transform, thumbnailWidth, thumbnailHeight, true
            );
        //check if the thumbnail was created
        if (!modelThumbnail)
        {
            DebugConsole.Instance.LogError($"couldnt generate thumbail for tempModel: {tempModel.name}");
        }
        //destroy the temp model
        Destroy(tempModel);

        return modelThumbnail;
        
    }


}
