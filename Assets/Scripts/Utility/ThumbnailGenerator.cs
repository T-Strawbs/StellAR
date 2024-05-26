using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ThumbnailGenerator : Singleton<ThumbnailGenerator>
{
    [SerializeField] private int thumbnailWidth = 32;
    [SerializeField] private int thumbnailHeight = 32;
    public Texture2D getModelThumbnail(Transform model)
    {
        Texture2D modelThumbnail = null;
        modelThumbnail = RuntimePreviewGenerator.GenerateModelPreview(model,thumbnailWidth,thumbnailHeight,true);
        if(!modelThumbnail)
        {
            DebugConsole.Instance.LogError($"couldnt generate thumbail for model: {model.name}");
        }
        return modelThumbnail;
    }

}
