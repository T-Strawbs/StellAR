using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;

public class MetadataManager : MonoBehaviour, Subscriber
{
    // Object housing all the models loaded in the application
    public GameObject allModelObjects;

    public GameObject metadataNearMenu;

    public GameObject selectionManager;

    // Start is called before the first frame update
    void Start()
    {

        selectionManager.GetComponent<SelectionManager>().addSubscriber(this);

        List<ModelComponent> allModels = new List<ModelComponent>();

        // Loop over all models
        foreach(Transform modelTransform in allModelObjects.transform)
        {
            modelTransform.gameObject.AddComponent<Metadata>();

            // Create a Component object for the model
            ModelComponent parentModel = new ModelComponent(modelTransform.name);

            /* 
             * Check if JSON representation already exists, if not create a blank template
             * NOTE: If the model architecture changes, the existing JSON will need to be deleted to create
             *          an accurate representation of the new architecture.
             */
            string jsonFilePath = Application.persistentDataPath + "/" + modelTransform.name + ".json";
            if (File.Exists(jsonFilePath))
            {
                // if JSON already exists add data from JSON, which also updated the JSON to any changed in the model if there are any
                string modelJson = File.ReadAllText(jsonFilePath);
                parentModel = JsonConvert.DeserializeObject<ModelComponent>(modelJson);
                addMetadataFromJson(modelTransform, parentModel);
                writeJSON(parentModel);
            }
            else
            {
                // if JSON doesn't exist create fresh JSON template
                createModelJson(modelTransform, parentModel);
                allModels.Add(parentModel);
                writeJSON(parentModel);
            }
        }
    }

    void writeJSON(ModelComponent model)
    {
        string jsonFilePath = Application.persistentDataPath + "/" + model.name + ".json";
        File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(model, Formatting.Indented));
    }

    // Use recursion to create a correctly nested representation of an input model using Component objects
    void createModelJson(Transform parentTransform, ModelComponent parentComponent)
    {
        // For each subcomponent in the input parent, create a new Component object and add it as a child of the parent
        foreach (Transform subcomponentTransform in parentTransform)
        {
            subcomponentTransform.gameObject.AddComponent<Metadata>();
            ModelComponent subcomponent = new ModelComponent(subcomponentTransform.name);
            parentComponent.subcomponents.Add(subcomponent);

            // Repeat recursion using newly created child as the new parent
            createModelJson(subcomponentTransform, subcomponent);
        }
    }

    void addMetadataFromJson(Transform parentTransform, ModelComponent parentComponent)
    {
        // add metadata to parent first
        parentTransform.GetComponent<Metadata>().metadata = parentComponent.metadata;

        // get list of subcomponent transforms, items will be deleted as they are linked to
        // their JSON counterparts and any remaining in the list will need to be added to the JSON
        List<Transform> subcomponentTransforms = new List<Transform>();
        foreach (Transform transform in parentTransform)
        {
            subcomponentTransforms.Add(transform);
        }

        List<ModelComponent> subcomponents = new List<ModelComponent>(parentComponent.subcomponents);

        foreach(ModelComponent subcomponent in subcomponents)
        {
            GameObject foundChild = parentTransform.gameObject.GetNamedChild(subcomponent.name);

            // if component in JSON is no longer in the model remove it from the JSON
            if(foundChild is null)
            {
                parentComponent.subcomponents.Remove(subcomponent);
            }

            // else populate the subcomponent with the metadata from the JSON
            else
            {
                Metadata subcomponentMetadata = foundChild.AddComponent<Metadata>();
                subcomponentMetadata.metadata = subcomponent.metadata;

                // remove JSON linked subcomponent from list
                subcomponentTransforms.Remove(foundChild.transform);

                // populate children components too
                addMetadataFromJson(foundChild.transform, subcomponent);
            }
        }

        // loop over components not in JSON
        foreach (Transform leftoverSubcomponent in subcomponentTransforms)
        {
            // initialise components with metadata and by adding as child to parent Component
            leftoverSubcomponent.gameObject.AddComponent<Metadata>();
            ModelComponent subcomponent = new ModelComponent(leftoverSubcomponent.name);
            parentComponent.subcomponents.Add(subcomponent);
            
            // check and add children of this component
            createModelJson(leftoverSubcomponent, subcomponent);
        }

    }

    public void UpdateSubscriber(Explodable newSelection)
    {
        TextMeshProUGUI text = metadataNearMenu.GetComponentInChildren<TextMeshProUGUI>();
        string newMetadata = newSelection.GetComponent<Metadata>().metadata;

        if(newMetadata != null && newMetadata != "")
        {
            text.text = newMetadata;
        }
        else
        {
            text.text = "No available metadata.";
        }
    }

    public void ToggleNearMenu()
    {
        metadataNearMenu.SetActive(!metadataNearMenu.activeInHierarchy);
    }
}
