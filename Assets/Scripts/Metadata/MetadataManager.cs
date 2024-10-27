using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;

public class MetadataManager : Singleton<MetadataManager>, PrefabLoadListener
{
    private void Awake()
    {
        PrefabManager.Instance.OnPrefabsLoaded.AddListener(onPrefabsLoaded);

        //check if the Metadata directtory exists and create it if it doesnt
        if (!Directory.Exists(GlobalConstants.METADATA_DIR))
        {
            Directory.CreateDirectory(GlobalConstants.METADATA_DIR);
        }
    }

    public void onPrefabsLoaded(List<GameObject> loadedPrefabs)
    {
        initialseMetadata(loadedPrefabs);
    }

    private void initialseMetadata(List<GameObject> loadedPrefabs)
    {
        List<MetadataJson> allModels = new List<MetadataJson>();
        DebugConsole.Instance.LogDebug($"ABOUT TO LOAD METADATA");

        // Loop over all models
        foreach (GameObject model in loadedPrefabs)
        {
            model.gameObject.AddComponent<MetadataComponent>();

            // Create a Component object for the model
            MetadataJson parentModel = new MetadataJson(model.name);

            /* 
             * Check if JSON representation already exists, if not create a blank template
             * NOTE: If the model architecture changes, the existing JSON will need to be deleted to create
             *          an accurate representation of the new architecture.
             */

            DebugConsole.Instance.LogDebug($"ATTEMPTING TO LOAD METADATA FOR {model.name}");

            string jsonFilePath = GlobalConstants.METADATA_DIR + model.name + "_Metadata.json";
            if(File.Exists(jsonFilePath))
            {
                // if JSON already exists add data from JSON, which also updated the JSON to any changed in the model if there are any
                string modelJson = File.ReadAllText(jsonFilePath);
                parentModel = JsonConvert.DeserializeObject<MetadataJson>(modelJson);
                addMetadataFromJson(model.transform, parentModel);
                writeJSON(parentModel);
                DebugConsole.Instance.LogDebug($"Found METADATA FOR {model.name}");
            }
            else
            {
                DebugConsole.Instance.LogDebug($"Couldnt find METADATA for {model.name}, should be creating some");
                // if JSON doesn't exist create fresh JSON template
                createModelJson(model.transform, parentModel);
                allModels.Add(parentModel);
                writeJSON(parentModel);
            }
        }
        DebugConsole.Instance.LogDebug($"Finished loading METADATA");
    }

    void writeJSON(MetadataJson model)
    {
        string jsonFilePath = GlobalConstants.METADATA_DIR + model.name + "_Metadata.json";
        File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(model, Formatting.Indented));
        DebugConsole.Instance.LogDebug($"Shouldve written metadata for {model.name}");

    }

    // Use recursion to create a correctly nested representation of an input model using Component objects
    void createModelJson(Transform parentTransform, MetadataJson parentComponent)
    {
        // For each subcomponent in the input parent, create a new Component object and add it as a child of the parent
        foreach (Transform subcomponentTransform in parentTransform)
        {
            subcomponentTransform.gameObject.AddComponent<MetadataComponent>();
            MetadataJson subcomponent = new MetadataJson(subcomponentTransform.name);
            parentComponent.subcomponents.Add(subcomponent);

            // Repeat recursion using newly created child as the new parent
            createModelJson(subcomponentTransform, subcomponent);
        }
    }

    void addMetadataFromJson(Transform parentTransform, MetadataJson parentComponent)
    {
        // add metadata to parent first
        parentTransform.GetComponent<MetadataComponent>().metadata = parentComponent.metadata;

        // get list of subcomponent transforms, items will be deleted as they are linked to
        // their JSON counterparts and any remaining in the list will need to be added to the JSON
        List<Transform> subcomponentTransforms = new List<Transform>();
        foreach (Transform transform in parentTransform)
        {
            subcomponentTransforms.Add(transform);
        }

        List<MetadataJson> subcomponents = new List<MetadataJson>(parentComponent.subcomponents);

        foreach(MetadataJson subcomponent in subcomponents)
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
                MetadataComponent subcomponentMetadata = foundChild.AddComponent<MetadataComponent>();
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
            leftoverSubcomponent.gameObject.AddComponent<MetadataComponent>();
            MetadataJson subcomponent = new MetadataJson(leftoverSubcomponent.name);
            parentComponent.subcomponents.Add(subcomponent);
            
            // check and add children of this component
            createModelJson(leftoverSubcomponent, subcomponent);
        }

    }

    
}
