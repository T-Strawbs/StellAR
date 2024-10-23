using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelPane : ScrollPane,ImportListener
{
    [SerializeField] protected ModelUI contentPrefab;
    [SerializeField] private List<ModelUI> uiElements = new List<ModelUI>();
    [SerializeField] private RectTransform defaultElement;

    private void Awake()
    {
        NetworkInteractablePrefabManager.Instance.OnImportCompleted.AddListener(OnImportComplete);
    }

    public void OnImportComplete(List<GameObject> importedObjects)
    {
        if (importedObjects.Count < 1)
        {
            defaultElement.gameObject.SetActive(true);
            return;
        }
        defaultElement.gameObject.SetActive(false);
        //for each model in the _/test_objects transform
        foreach(GameObject importedObject in importedObjects)
        {
            //instantiate content prefab
            ModelUI content = Instantiate<ModelUI>(contentPrefab);
            //set the parent and local transforms of content
            content.transform.SetParent(contentHolder);
            content.transform.localPosition = Vector3.zero;
            content.transform.localRotation = Quaternion.identity;
            content.transform.localScale = Vector3.one;
            //attempt to generate thumbnail
            Texture2D thumbnail = ThumbnailGenerator.Instance.getModelThumbnail(importedObject);
            //initialise content
            content.initialise(importedObject, thumbnail);
            uiElements.Add(content);
        }
    }

    public override void populateScrollPane()
    {
        // we should do something about the abstraction becuase its garbage
    }

    
}
