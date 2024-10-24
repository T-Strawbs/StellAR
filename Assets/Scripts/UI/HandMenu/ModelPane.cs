using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelPane : ScrollPane
{
    [SerializeField] protected ModelUI contentPrefab;
    [SerializeField] private List<ModelUI> uiElements = new List<ModelUI>();
    [SerializeField] private RectTransform defaultElement;

    public void populateScrollPane(List<GameObject> importedObjects)
    {
        DebugConsole.Instance.LogDebug("HELLO FROM THE MENU PANE");
        Debug.Log("LOGGER HELLO");
        if (importedObjects.Count < 1)
        {
            defaultElement.gameObject.SetActive(true);
            return;
        }
        defaultElement.gameObject.SetActive(false);
        //for each model that the PrefabManager has imported
        for (int i = 0; i < importedObjects.Count; i++)
        {
            GameObject importedObject = importedObjects[i];
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
            content.initialise(i, importedObject.name, thumbnail);
            uiElements.Add(content);
        }
    }

    public override void populateScrollPane()
    {
        throw new System.NotImplementedException();
    }
}
