
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using static Unity.VisualScripting.AnnotationUtility;

public class AnnotationManager : Singleton<AnnotationManager>, PrefabLoadListener
{
    //this allows us to ensure that the annotation json subtypes are accepted
    private JsonSerializerSettings settings;
    private void Awake()
    {
        //subscribe to the prefab managers OnPrefabsLoaded event 
        PrefabManager.Instance.OnPrefabsLoaded.AddListener(onPrefabsLoaded);

        //ensure that the annotation json concretions are accepted
        settings = new JsonSerializerSettings
        { 
            //TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };
    }

    /// <summary>
    /// Creates the annotation directory if it doesnt exist already
    /// </summary>
    private void setupAnnotationDirectory()
    {
        //if the annotations dir doesnt exist
        if (!Directory.Exists(GlobalConstants.ANNOTATION_DIR))
            //create it
            Directory.CreateDirectory(GlobalConstants.ANNOTATION_DIR);
    }

    /// <summary>
    /// Creates the annotation subdirectory for the model, using the models name, if it doesnt exist already
    /// </summary>
    private void setupAnnotationSubdirectory(string modelName)
    {
        //check if an annotation subdirectory exists for the prefab and if not, create one
        if (!Directory.Exists($"{GlobalConstants.ANNOTATION_DIR}/{modelName}/"))
            Directory.CreateDirectory($"{GlobalConstants.ANNOTATION_DIR}/{modelName}/");

    }

    public void onPrefabsLoaded(List<GameObject> loadedPrefabs)
    {
        initialiseAnnotations(ref loadedPrefabs);
    }

    /// <summary>
    /// initialises the annotation data and components for all loaded prefabs
    /// </summary>
    /// <param name="loadedPrefabs"></param>
    private void initialiseAnnotations(ref List<GameObject> loadedPrefabs)
    {
        //create the annotation directory if it doesnt exist
        setupAnnotationDirectory();

        //for each model in our model list
        foreach (GameObject modelRoot in loadedPrefabs)
        {
            //create an annotation subdirectory for this prefab if it doesnt have one
            setupAnnotationSubdirectory(modelRoot.name);

            //Create the Serialised Annotation
            ModelAnnotationJson parentAnnotationJson = new ModelAnnotationJson(modelRoot.name);

            //check if the modelname_Annotation.json exists
            string jsonPath = $"{GlobalConstants.ANNOTATION_DIR}/{modelRoot.name}/{modelRoot.name}_Annotation.json";
            if (File.Exists(jsonPath))
            {
                //json exists so we load annotation data from the json file
                string annotationJson = File.ReadAllText(jsonPath);
                //deserialise the data from json into usable objects
                parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson, settings);
                //Populate The models GO with the Json Data
                populateAnnotationDataFromJson(modelRoot.transform, parentAnnotationJson);
                //write to json to update the content incase the model has changed
                writeJson(parentAnnotationJson, jsonPath);
            }
            else
            {
                //json doesnt exist so we have to create it
                createAnnotationJsonObject(modelRoot.transform, parentAnnotationJson);
                //write annoation data out to file
                writeJson(parentAnnotationJson, jsonPath);
            }
        }
    }

    /// <summary>
    /// recursively populates the annotation data components of a model using its annotation json file
    /// </summary>
    /// <param name="parentTransform"></param>
    /// <param name="parentComponent"></param>
    private void populateAnnotationDataFromJson(Transform parentTransform, ModelAnnotationJson parentComponent)
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
            createAnnotationJsonObject(leftoverSubcomponent, subcomponent);
        }

    }

    /// <summary>
    /// recursively creates the serialised json object for the node of a given model
    /// </summary>
    /// <param name="parentTransform"></param>
    /// <param name="parentComponent"></param>
    private void createAnnotationJsonObject(Transform parentTransform,ModelAnnotationJson parentComponent)
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
            createAnnotationJsonObject(childTransform, childAnnotationJson);
        }
    }

    /// <summary>
    /// deserialises the model json object tree and writes it to json
    /// </summary>
    /// <param name="serialisedAnnotation"></param>
    /// <param name="jsonPath"></param>
    private void writeJson(ModelAnnotationJson serialisedAnnotation, string jsonPath)
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

    /// <summary>
    /// creates a serialised json object for anotation messages 
    /// </summary>
    /// <param name="componentName"></param>
    /// <param name="messageType"></param>
    /// <param name="author"></param>
    /// <param name="dateTime"></param>
    /// <param name="content"></param>
    public void createAnnotationJson(string componentName,string messageType,string author, string dateTime,string content)
    {
        DebugConsole.Instance.LogDebug($"Creating annotation for {componentName}");
        //Create null annotaion reference
        AnnotationJson annotationJson = null;
        //check the message type
        if (messageType == "Text")
        {
            DebugConsole.Instance.LogDebug($"created TextAnnotation");
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
            DebugConsole.Instance.LogDebug($"created VoiceAnnotation");
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
            DebugConsole.Instance.LogError($"Cant create annotation for {componentName} " +
                $"as the message type {messageType} is invalid");
            return;
        }
        //write to json and add annotation to the AnnotationComponent
        addAnnotationToJson(annotationJson);
    }

    /// <summary>
    /// adds the serialised annotation message object to the models annotation json file
    /// </summary>
    /// <param name="annotation"></param>
    private void addAnnotationToJson(AnnotationJson annotation)
    {
        
        DebugConsole.Instance.LogDebug($"Adding annoation to json for {annotation.ComponentName}");
        //grab the current selectable objects transform
        Transform currentSelection = SelectionManager.Instance.currentSelection.transform;
        //
        if(!currentSelection)
        {
            DebugConsole.Instance.LogError("cannot add annotation to json as there is no current selectable");
            return;
        }
        //add the annotation to the current selections annotation component's list
        AnnotationComponent annotationComponent = currentSelection.GetComponent<AnnotationComponent>();
        if(!annotationComponent)
        {
            DebugConsole.Instance.LogError("cannot add annotation to json as there is no annotation " +
                "component on the current selectable");
            return;
        }
        //add annotation 
        annotationComponent.Annotations.Add(annotation);
        Transform currentSelectionModelRoot;
        try 
        {
            currentSelectionModelRoot = SelectionManager.Instance.getSelectionRootTransform(); 
        }
        catch (Exception e)
        {
            DebugConsole.Instance.LogError($"Cannot add annotation as the theres no parent of the current" +
                $" selection ({currentSelection.name}) somehow?");
            return;
        }

        //get the parent of the current object 
        string fileName =  $"{GlobalConstants.ANNOTATION_DIR}/{currentSelectionModelRoot.name}/{currentSelectionModelRoot.name}_Annotation.json";
        if(!File.Exists(fileName))
        {
            DebugConsole.Instance.LogError($"Cannot add annotation data as file:{fileName} cannot be found");
            return;
        }
        DebugConsole.Instance.LogDebug($"file exists:{fileName}");
        //get the json content
        string annotationJson = File.ReadAllText(fileName);
        DebugConsole.Instance.LogDebug($"loading file content:{fileName}");
        //get the deserialised json object from the content
        ModelAnnotationJson parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson);
        //update the json object to now include the new annotation
        updateAnnotationJson(currentSelectionModelRoot, parentAnnotationJson, currentSelection.name, annotation);
        DebugConsole.Instance.LogDebug($"Attempting to write to json");
        //write to the json file
        writeJson(parentAnnotationJson, fileName);
        
    }

    // call whenever a component's highlight colour is updated. Updates the Json file accordingly
    /// <summary>
    /// updates the model annotation json to use the new highlight colour
    /// </summary>
    /// <param name="highlightColour"></param>
    public void updateAnnotationHighlightJson(string highlightColour)
    {
        Transform currentSelectionModelRoot;
        try
        {
            currentSelectionModelRoot = SelectionManager.Instance.getSelectionRootTransform();
        }
        catch (Exception e)
        {
            DebugConsole.Instance.LogError($"Cannot add annotation as the theres no parent of the current" +
                $" selection ({SelectionManager.Instance.currentSelection.name}) somehow?");
            return;
        }

        // highlighted object will be currently selected
        string parentJsonFileName = $"{GlobalConstants.ANNOTATION_DIR}/{currentSelectionModelRoot.name}/{currentSelectionModelRoot.name}_Annotation.json";
        string targetName = SelectionManager.Instance.currentSelection.name;
        string parentJsonFile = File.ReadAllText(parentJsonFileName);

        // recreate Json object structure in memory
        ModelAnnotationJson parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(parentJsonFile, settings);
        ModelAnnotationJson targetJson;

        //check if the parent is the highlighted component
        if (parentAnnotationJson.Name == targetName)
        {
            targetJson = parentAnnotationJson;
        }
        else
        {
            // else find the corresponding subcomponent in the Json structure
            targetJson = findSubcomponentInJson(parentAnnotationJson, targetName);

        }

        // if we found a corresponding subcomponent update its colour value and all children too, then write out changes
        if (targetJson != null)
        {
            // loop through and update highlight value of children too
            updateHighlightOfChildren(targetJson, highlightColour);
            writeJson(parentAnnotationJson, parentJsonFileName);
        }
        else
        {
            DebugConsole.Instance.LogError($"couldnt find found {targetName}");
        }
        
    }

    // used to update the highlight colour value of all subcomponents of a component
    /// <summary>
    /// recursively updates the highlight colour of the give model annotation json object and children
    /// </summary>
    /// <param name="parentAnnotationJson"></param>
    /// <param name="highlightColour"></param>
    private void updateHighlightOfChildren(ModelAnnotationJson parentAnnotationJson, string highlightColour)
    {
        // update the highlight colour of current component
        parentAnnotationJson.HighlightColour = highlightColour;

        // do the same to all subcomponents recursively
        foreach (ModelAnnotationJson childAnnotationJson in parentAnnotationJson.Subcomponents)
        {
            updateHighlightOfChildren(childAnnotationJson, highlightColour);
        }
    }
    /// <summary>
    /// finds and returns the target subcomponent in the model annotation object
    /// </summary>
    /// <param name="parentAnnotationJson"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    private ModelAnnotationJson findSubcomponentInJson(ModelAnnotationJson parentAnnotationJson, string targetName)
    {
        ModelAnnotationJson returnValue = null;

        if (parentAnnotationJson.Name == targetName)
        {
            return parentAnnotationJson;
        }
        else
        {
            foreach (ModelAnnotationJson subcomponent in parentAnnotationJson.Subcomponents)
            {
                returnValue = findSubcomponentInJson(subcomponent, targetName);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
        }
        return returnValue;
    }
    /// <summary>
    /// updates the changes made to the model annotation json object to the models annotation json file
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="parentJson"></param>
    /// <param name="targetName"></param>
    /// <param name="annotation"></param>
    private void updateAnnotationJson(Transform parent, ModelAnnotationJson parentJson,string targetName, AnnotationJson annotation)
    {
        DebugConsole.Instance.LogDebug($"updateing json attempting to find the targetname:{targetName}" +
            $" inside {parent.name}");

        //check if the parents name matches the target name
        if (parentJson.Name == targetName)
        {
            DebugConsole.Instance.LogDebug($"found {targetName} so we are adding the annotation");
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
                DebugConsole.Instance.LogDebug($"found the next child to search from parent:" +
                    $"{parent.name} to child {foundChild.name}");
                //recursively call this method to find where to put the annotation
                updateAnnotationJson(foundChild.transform, subcomponent, targetName, annotation);
            }
        }
        DebugConsole.Instance.LogError($"couldnt find found {targetName}");
    }
    /// <summary>
    /// Deletes the target annotation message data from the models annotation json file
    /// </summary>
    /// <param name="annotationData"></param>
    public void deleteAnnotation(AnnotationJson annotationData)
    {
        
        //check if we have a currentSelection
        if(!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError($"Cannot Delete Annotation ({annotationData.Author}:{annotationData.Timestamp}) " +
                $"as theres no current selection");
            return;
        }
        //find the name of the components root predecessor
        string rootName = SelectionManager.Instance.getSelectionRootTransform().name;
        if (rootName == "")
        {
            DebugConsole.Instance.LogError($"Cannot find root predecessor of {SelectionManager.Instance.currentSelection.name}");
            return;
        }
        //load in the model json
        string jsonPath = $"{GlobalConstants.ANNOTATION_DIR}/{rootName}/{rootName}_Annotation.json";
        if(!File.Exists(jsonPath))
        {
            DebugConsole.Instance.LogError($"couldnt find file:{jsonPath}");
            return;
        }
        //deserialise the json into the ModelAnnotation object
        ModelAnnotationJson rootJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(File.ReadAllText(jsonPath), settings);
        //search the root json for the currently selected component
        ModelAnnotationJson targetJson = findComponent(rootJson,SelectionManager.Instance.currentSelection.name);
        if(targetJson == null)
        {
            DebugConsole.Instance.LogError($"Couldnt find {SelectionManager.Instance.currentSelection.name} in {rootJson.Name}");
            return;
        }
        DebugConsole.Instance.LogDebug($"this annotation comps anno count is{targetJson.Annotations.Count}");
        //find the target annotation in the target jsons annotations
        for (int i = 0; i < targetJson.Annotations.Count; i++)
        {
            DebugConsole.Instance.LogDebug($"checking if (" +
                $"{targetJson.Annotations[i].ComponentName},{targetJson.Annotations[i].Author},{targetJson.Annotations[i].Timestamp})" +
                $" is equal to " +
                $"({annotationData.ComponentName},{annotationData.Author},{annotationData.Timestamp})");
            if (targetJson.Annotations[i].Equals(annotationData))
            {
                DebugConsole.Instance.LogDebug("The annotations match in current selection");
                targetJson.Annotations.RemoveAt(i);
                break;
            }
        }
        //remove the annotation from the current selections annotation component
        AnnotationComponent currentSelection = SelectionManager.Instance.currentSelection.GetComponent<AnnotationComponent>();
        if (!currentSelection)
        {
            DebugConsole.Instance.LogError($"Couldnt find {SelectionManager.Instance.currentSelection.name} annotation component");
            return;
        }
        for(int i = 0; i < currentSelection.Annotations.Count; i++)
        {
            if (currentSelection.Annotations[i].Equals(annotationData))
            {
                DebugConsole.Instance.LogDebug("The annotations match in current selection");
                currentSelection.Annotations.RemoveAt(i);
                break;
            }
        }
        DebugConsole.Instance.LogDebug($"printing annotation authors of current selection");
        //debug
        foreach (AnnotationJson annotation in currentSelection.Annotations)
        {
            DebugConsole.Instance.LogDebug($"{annotation.Author}");
        }
        DebugConsole.Instance.LogDebug($"finished printing annotation authors of current selection");
        //update json
        writeJson(rootJson, jsonPath);
        //update DataPanelManager
        DataPanelManager.Instance.updateAnnotations(currentSelection);
       
    }

    /// <summary>
    /// finds and returns the model component matches the target name
    /// </summary>
    /// <param name="currentModelJson"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    private ModelAnnotationJson findComponent(ModelAnnotationJson currentModelJson,string targetName)
    {
        if(currentModelJson.Name == targetName)
        {
            return currentModelJson;
        }
        foreach(ModelAnnotationJson subcomponent in currentModelJson.Subcomponents)
        {
            ModelAnnotationJson result =  findComponent(subcomponent, targetName);
            if(result != null)
                return result;
        }
        return null;
    }
    /// <summary>
    /// recursive search for the model component matching the target name
    /// </summary>
    /// <param name="currentComponent"></param>
    /// <param name="targetName"></param>
    /// <param name="target"></param>
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
