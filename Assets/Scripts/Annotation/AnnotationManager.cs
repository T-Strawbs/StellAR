using JsonSubTypes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;

public class AnnotationManager : Singleton<AnnotationManager>
{
    [SerializeField] private List<Transform> models;
    [SerializeField] string jsonDirPath;

    //this allows us to ensure that the annotation json subtypes are accepted
    private JsonSerializerSettings settings;
    private void Awake()
    {
        jsonDirPath = $"{Application.persistentDataPath}/";

        //ensure that the annotation json concretions are accepted
        settings = new JsonSerializerSettings
        { 
            //TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };

    }

    // Start is called before the first frame update
    void Start()
    {
        initialiseAnnotations();
    }

    private void initialiseAnnotations()
    {
        //for each model in our model list
        foreach (Transform modelRoot in models)
        {
            //add annotation component to the root of the model
            //model.AddComponent<AnnotationComponent>();

            //Create the Serialised Annotation
            ModelAnnotationJson parentAnnotationJson = new ModelAnnotationJson(modelRoot.name);

            //check if the modelname_Annotation.json exists
            string jsonPath = $"{jsonDirPath}{modelRoot.name}_Annotation.json";
            if (File.Exists(jsonPath))
            {
                //json exists so we load annotation data from the json file
                string annotationJson = File.ReadAllText(jsonPath);
                //deserialise the data from json into usable objects
                parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson,settings);
                //Populate The models GO with the Json Data
                populateAnnotationDataFromJson(modelRoot, parentAnnotationJson);
                //write to json to update the content incase the model has changed
                writeJson(parentAnnotationJson,jsonPath);
            }
            else
            {
                //json doesnt exist so we have to create it
                createAnnotationJson(modelRoot, parentAnnotationJson);
                //write annoation data out to file
                writeJson(parentAnnotationJson, jsonPath);
            }
        }
    }

    private void populateAnnotationDataFromJson(Transform parentTransform,ModelAnnotationJson parentComponent)
    {
        //create a new annotation component for the parent
        AnnotationComponent annotationComponent = parentTransform.AddComponent<AnnotationComponent>();
        //add the annotation data to the annotation component
        annotationComponent.Annotations = parentComponent.Annotations;

        //get list of subcomponent transforms
        List<Transform> subcomponentTransforms = new List<Transform>();
        foreach(Transform subcomponent in parentTransform)
        {
            subcomponentTransforms.Add(subcomponent);
        }

        //-- removing dead json links
        //for each subcompnent in the parents subcomponents
        foreach(ModelAnnotationJson subcomponent in parentComponent.Subcomponents)
        {
            //find the child of the parent transofrm matching the subcomponent name
            GameObject foundChild = parentTransform.gameObject.GetNamedChild(subcomponent.Name);

            // if component in JSON is no longer in the model remove it from the JSON
            if (foundChild is null)
            {
                parentComponent.Subcomponents.Remove(subcomponent);
                continue;
            }
            /*
            //populate the subcomponent with the Annotation data from the JSON
            AnnotationComponent subcomponentAnnotations = foundChild.AddComponent<AnnotationComponent>();
            subcomponentAnnotations.Annotations = subcomponent.Annotations;
            */
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

    private void createAnnotationJson(Transform parentTransform,ModelAnnotationJson parentComponent)
    {
        
        //add a Annotation component to the parent
        parentTransform.AddComponent<AnnotationComponent>();
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

    private void writeJson(ModelAnnotationJson serialisedAnnotation, string jsonPath)
    {
        //write out annotation data to json
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(serialisedAnnotation,settings));
    }

    public void createAnnotationJson(string componentName,string messageType,string author, string dateTime,string content)
    {
        Debug.Log($"Creating annotation for {componentName}");
        //Create null annotaion reference
        AnnotationJson annotationJson = null;
        //check the message type
        if (messageType == "Text")
        {
            Debug.Log($"created TextAnnotation");
            //create a new serialisble text annotation object and populate it with the params
            annotationJson = new TextAnnotationJson
            {
                ComponentName = componentName,
                Author = author,
                Timestamp = dateTime,
                Content = content
            };
        }
        else if(messageType == "Voice")
        {
            Debug.Log($"created VoiceAnnotation");
            //create a new serialisble voice annotation object and populate it with the params
            annotationJson = new VoiceAnnotationJson
            {
                ComponentName = componentName,
                Author = author,
                Timestamp = dateTime,
                AudioPath = content
            };
            
        }
        else
        {
            Debug.Log($"Cant create annotation for {componentName} as the message type {messageType} is invalid");
            return;
        }
        //write to json and add annotation to the AnnotationComponent
        addAnnotationToJson(annotationJson);
    }

    private void addAnnotationToJson(AnnotationJson annotation)
    {
        Debug.Log($"Adding annoation to json for {annotation.ComponentName}");
        //grab the current selectable objects transform
        Transform currentSelection = SelectionManager.currentSelection.transform;
        //
        if(!currentSelection)
        {
            Debug.Log("cannot add annotation to json as there is no current selectable");
            return;
        }
        //add the annotation to the current selections annotation component's list
        AnnotationComponent annotationComponent = currentSelection.GetComponent<AnnotationComponent>();
        if(!annotationComponent)
        {
            Debug.Log("cannot add annotation to json as there is no annotation component on the current selectable");
            return;
        }
        //add annotation 
        annotationComponent.Annotations.Add(annotation);
        //get the parent of the current object
        Transform currentSelectionParent = SelectionManager.Instance.getCurrentSelectionParent();
        if(!currentSelectionParent)
        {
            Debug.Log($"Cannot add annotation as the theres no parent of the current selection ({currentSelection.name}) somehow?");
            return;
        }
        string fileName = $"{Application.persistentDataPath}/{currentSelectionParent.name}_Annotation.json";
        if(!File.Exists(fileName))
        {
            Debug.Log($"Cannot add annotation data as file:{fileName} cannot be found");
            return;
        }
        Debug.Log($"file exists:{fileName}");
        //get the json content
        string annotationJson = File.ReadAllText(fileName);
        Debug.Log($"loading file content:{fileName}");
        //get the deserialised json object from the content
        ModelAnnotationJson parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson);
        //update the json object to now include the new annotation
        updateAnnotationJson(currentSelectionParent, parentAnnotationJson, currentSelection.name, annotation);
        Debug.Log($"Attempting to write to json");
        //write to the json file
        writeJson(parentAnnotationJson, fileName);
    }

    private void updateAnnotationJson(Transform parent, ModelAnnotationJson parentJson,string targetName, AnnotationJson annotation)
    {
        Debug.Log($"updateing json attempting to find the targetname:{targetName} inside {parent.name}");

        //check if the parents name matches the target name
        if (parentJson.Name == targetName)
        {
            Debug.Log($"found {targetName} so we are adding the annotation");
            //add the annotation to the subcomponent's annotations
            parentJson.Annotations.Add(annotation);
            return;
        }

        // for each subcomponent in the parent object
        foreach (ModelAnnotationJson subcomponent in parentJson.Subcomponents)
        {
            //find the child of the parent tranform that matches the target name
            GameObject foundChild = parent.gameObject.GetNamedChild(subcomponent.Name);
            // if we found a child that matches the corresponding subcomponent name
            if(foundChild)
            {
                Debug.Log($"found the next child to search from parent:{parent.name} to child {foundChild.name}");
                //recursively call this method to find where to put the annotation
                updateAnnotationJson(foundChild.transform, subcomponent, targetName, annotation);
            }
        }
        Debug.Log($"couldnt find found {targetName}");
    }

    private void findComponent(Transform currentComponent, string targetName, Transform target)
    {
        //check the current components name
        if(currentComponent.name == targetName)
        {
            target = currentComponent;
            return;
        }
        //for each sub component
        for(int i = 0; i < currentComponent.childCount; i++)
        {
            Transform subcomponent = currentComponent.GetChild(i);
            if (!subcomponent)
                continue;
            findComponent(subcomponent,targetName,target);
        }

    }
}
