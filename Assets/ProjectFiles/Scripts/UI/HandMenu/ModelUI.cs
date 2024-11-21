using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class representing the UI element for displaying a model's name and thumbnail as a pressable button
/// in order to spawn that model.
/// </summary>
public class ModelUI : MonoBehaviour
{
    /// <summary>
    /// The thumbnail image of the given model
    /// </summary>
    [SerializeField] private Image thumbnail;
    /// <summary>
    /// The text UI element to display the name 
    /// of the given model
    /// </summary>
    [SerializeField] private TMP_Text modelName;
    /// <summary>
    /// The models index position in the prefab managers
    /// spawnablePrefabs collection
    /// </summary>
    [SerializeField] private int prefabIndex;
    /// <summary>
    /// The button that begins the spawnning of the given model.
    /// </summary>
    [SerializeField] private PressableButton importBtn;
    
    /// <summary>
    /// Method for initialising this ModelUI
    /// </summary>
    /// <param name="prefabIndex"></param>
    /// <param name="prefabName"></param>
    /// <param name="thumbnail"></param>
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
                    PrefabManager.Instance.requestInteractbleSpawn(this.prefabIndex);
                }
            );
    }
}
