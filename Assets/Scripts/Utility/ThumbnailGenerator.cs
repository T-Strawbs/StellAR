using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
public class ThumbnailGenerator : Singleton<ThumbnailGenerator>
{
    [SerializeField] private int thumbnailWidth = 32;
    [SerializeField] private int thumbnailHeight = 32;
    public Texture2D getModelThumbnail(GameObject prefab)
    {
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
