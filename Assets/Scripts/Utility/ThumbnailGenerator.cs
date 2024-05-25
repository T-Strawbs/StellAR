using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ThumbnailGenerator : Singleton<ThumbnailGenerator>
{
    public Texture2D getModelThumbnail(Transform model)
    {
        Texture2D modelThumbnail = null;
        modelThumbnail = RuntimePreviewGenerator.GenerateModelPreview(model);
        if(!modelThumbnail)
        {
            DebugConsole.Instance.LogError($"couldnt generate thumbail for model: {model.name}");
        }
        return modelThumbnail;
    }

}
