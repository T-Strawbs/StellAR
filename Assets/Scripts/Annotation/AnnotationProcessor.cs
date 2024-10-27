#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AnnotationProcessor
{
    //this allows us to ensure that the annotation json subtypes are accepted
    private static JsonSerializerSettings settings = new JsonSerializerSettings
    {
        //TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.Auto,
    };

    static AnnotationProcessor()
    {
        initialiseAnnotationData();
    }

    public static void initialiseAnnotationData()
    {
        //create the annotation directory if it doesnt exist
        setupAnnotationDirectory();

        //get all the prefab paths from the prefab dir
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs");

        foreach(GameObject prefab in loadedPrefabs)
        {
            //create an annotation subdirectory for this prefab if it doesnt have one
            setupAnnotationSubdirectory(prefab.name);

            //Create the Serialised Annotation
            ModelAnnotationJson parentAnnotationJson = new ModelAnnotationJson(prefab.name);

            //check if the modelname_Annotation.json exists
            string jsonPath = $"{GlobalConstants.ANNOTATION_DIR}/{prefab.name}/{prefab.name}_Annotation.json";
            if (File.Exists(jsonPath))
            {
                //json exists so we load annotation data from the json file
                string annotationJson = File.ReadAllText(jsonPath);
                //deserialise the data from json into usable objects
                parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson, settings);
                //Populate The models GO with the Json Data
                populateAnnotationDataFromJson(prefab.transform, parentAnnotationJson);
                //write to json to update the content incase the model has changed
                writeJson(parentAnnotationJson, jsonPath);
            }
            else
            {
                //json doesnt exist so we have to create it
                createAnnotationJson(prefab.transform, parentAnnotationJson);
                //write annoation data out to file
                writeJson(parentAnnotationJson, jsonPath);
            }
        }
    }

    private static void populateAnnotationDataFromJson(Transform parentTransform, ModelAnnotationJson parentComponent)
    {
        //create a new annotation component for the parent
        AnnotationComponent annotationComponent = parentTransform.AddComponent<AnnotationComponent>();
        parentComponent.OriginalColourCode = annotationComponent.getOriginalolourString();
        annotationComponent.highlightColour = parentComponent.HighlightColour;
        //add the annotation data to the annotation component
        annotationComponent.Annotations = parentComponent.Annotations;

        //get list of subcomponent transforms
        List<Transform> subcomponentTransforms = new List<Transform>();
        foreach (Transform subcomponent in parentTransform)
        {
            subcomponentTransforms.Add(subcomponent);
        }

        //-- removing dead json links
        //for each subcompnent in the parents subcomponents
        foreach (ModelAnnotationJson subcomponent in parentComponent.Subcomponents)
        {
            //find the child of the parent transofrm matching the subcomponent name
            GameObject foundChild = parentTransform.gameObject.GetNamedChild(subcomponent.Name);

            // if component in JSON is no longer in the model remove it from the JSON
            if (foundChild is null)
            {
                parentComponent.Subcomponents.Remove(subcomponent);
                continue;
            }

            //populate the subcomponent with the Annotation data from the JSON
            AnnotationComponent subcomponentAnnotations = foundChild.AddComponent<AnnotationComponent>();
            subcomponentAnnotations.Annotations = subcomponent.Annotations;

            // remove JSON linked subcomponent from list
            subcomponentTransforms.Remove(foundChild.transform);

            // populate children components too
            populateAnnotationDataFromJson(foundChild.transform, subcomponent);

        }
        //-- adding new json links
        // loop over components not in JSON
        foreach (Transform leftoverSubcomponent in subcomponentTransforms)
        {
            // initialise components with metadata and by adding as child to parent Component
            leftoverSubcomponent.AddComponent<AnnotationComponent>();
            ModelAnnotationJson subcomponent = new ModelAnnotationJson(leftoverSubcomponent.name);
            parentComponent.Subcomponents.Add(subcomponent);

            // check and add children of this component
            createAnnotationJson(leftoverSubcomponent, subcomponent);
        }

    }

    private static void createAnnotationJson(Transform parentTransform, ModelAnnotationJson parentComponent)
    {

        //add a Annotation component to the parent
        AnnotationComponent annotationComponent = parentTransform.AddComponent<AnnotationComponent>();
        parentComponent.OriginalColourCode = annotationComponent.getOriginalolourString();

        //for each child of the parent
        foreach (Transform childTransform in parentTransform)
        {
            //create an empty Serialised Annotation object
            ModelAnnotationJson childAnnotationJson = new ModelAnnotationJson(childTransform.name);
            //add the Serialised annotation as a subcomponent of the parent
            parentComponent.Subcomponents.Add(childAnnotationJson);
            //recursively call for this childs decendants 
            createAnnotationJson(childTransform, childAnnotationJson);
        }
    }

    private static void writeJson(ModelAnnotationJson serialisedAnnotation, string jsonPath)
    {
        try
        {
            //write out annotation data to json
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(serialisedAnnotation, settings));
            DebugConsole.Instance.LogDebug($"created file: {jsonPath}");
        }
        catch (Exception e)
        {
            DebugConsole.Instance.LogError($"Cannot create file: {jsonPath}\n{e.ToString()}\n{e.StackTrace}\n{e.Message}");
        }

    }
    private static void setupAnnotationDirectory()
    {
        //if the annotations dir doesnt exist
        if (!Directory.Exists(GlobalConstants.ANNOTATION_DIR))
            //create it
            Directory.CreateDirectory(GlobalConstants.ANNOTATION_DIR);
    }

    private static void setupAnnotationSubdirectory(string prefabName)
    {
        //check if an annotation subdirectory exists for the prefab and if not, create one
        if (!Directory.Exists($"{GlobalConstants.ANNOTATION_DIR}/{prefabName}/"))
            Directory.CreateDirectory($"{GlobalConstants.ANNOTATION_DIR}/{prefabName}/");

    }
}
#endif // UNITYEDITOR