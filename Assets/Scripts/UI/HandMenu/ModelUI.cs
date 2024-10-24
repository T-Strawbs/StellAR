using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelUI : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TMP_Text modelName;
    [SerializeField] private int prefabIndex;
    [SerializeField] private PressableButton importBtn;
    
    public void initialise(int prefabIndex, string prefabName, Texture2D thumbnail)
    {
        //if the thumbnail exists then assign it as a sprite to our image
        if(thumbnail)
            this.thumbnail.sprite = Sprite.Create(
                thumbnail, 
                new Rect(0, 0, thumbnail.width, 
                thumbnail.height), 
                new Vector2(0.5f, 0.5f));
        //set the name text to the models name
        modelName.text = prefabName;
        this.prefabIndex = prefabIndex;

        //setup the listener event callback for importing models
        importBtn.OnClicked.AddListener
            (
                () => 
                {
                    Debug.Log($"Hello from the button for {modelName.text}");
                    NetworkInteractablePrefabManager.Instance.requestInteractbleSpawn(this.prefabIndex);
                }
            );
    }
}
