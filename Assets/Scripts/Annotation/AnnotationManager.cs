
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using Unity.XR.CoreUtils;
using UnityEngine;

public class AnnotationManager : Singleton<AnnotationManager>, PrefabInstantationListener
{
    //this allows us to ensure that the annotation json subtypes are accepted
    private JsonSerializerSettings settings;
    private void Awake()
    {
        PrefabManager.Instance.OnPrefabInstantiation.AddListener(onPrefabInstantiation);

        //ensure that the annotation json concretions are accepted
        settings = new JsonSerializerSettings
        {
            //TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        //create main directory if it doesn exist
        createAnnotationMainDirectory();
    }

    public void onPrefabInstantiation(GameObject instance)
    {
        initialiseAnnotations(instance);
    }

    private void createAnnotationMainDirectory()
    {
        //check if the main annotation dir exists and if not create it
        if(!Directory.Exists($"{GlobalConstants.ANNOTATION_DIR}/"))
        {
            Directory.CreateDirectory(GlobalConstants.ANNOTATION_DIR);
        }
    }

    private void createAnnotationSubDirectorDirectory(string rootModelName)
    {
        //check if the directory exists an if not create it
        if(!Directory.Exists($"{GlobalConstants.ANNOTATION_DIR}/{rootModelName}/"))
        {
            Directory.CreateDirectory($"{GlobalConstants.ANNOTATION_DIR}/{rootModelName}/");
        }
    }

    private void initialiseAnnotations(GameObject prefabInstance)
    {

        //create subsdirectory if it doesnt exist
        createAnnotationSubDirectorDirectory(prefabInstance.name);

        //Create the Serialised Annotation
        ModelAnnotationJson parentAnnotationJson = new ModelAnnotationJson(prefabInstance.name);

        //check if the modelname_Annotation.json exists
        string jsonPath = $"{GlobalConstants.ANNOTATION_DIR}/{prefabInstance.name}/{prefabInstance.name}_Annotation.json";
        if (File.Exists(jsonPath))
        {
            //json exists so we load annotation data from the json file
            string annotationJson = File.ReadAllText(jsonPath);
            //deserialise the data from json into usable objects
            parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson, settings);
            //Populate The models GO with the Json Data
            populateAnnotationDataFromJson(prefabInstance.transform, parentAnnotationJson);
            //write to json to update the content incase the model has changed
            writeJson(parentAnnotationJson, jsonPath);
        }
        else
        {
            //json doesnt exist so we have to create it
            createAnnotationJson(prefabInstance.transform, parentAnnotationJson);
            //write annoation data out to file
            writeJson(parentAnnotationJson, jsonPath);
        }
    }

    private void populateAnnotationDataFromJson(Transform parentTransform, ModelAnnotationJson parentComponent)
    {
        AnnotationComponent annotationComponent = parentTransform.GetComponent<AnnotationComponent>();
        //check if the object already has an annotation component and add it if it doesnt
        if (annotationComponent == null)
        {
            //create a new annotation component for the parent
            annotationComponent = parentTransform.AddComponent<AnnotationComponent>();
        }
        
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

    private void createAnnotationJson(Transform parentTransform, ModelAnnotationJson parentComponent)
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

    public void createAnnotationJson(string componentName, string messageType, string author, string dateTime, string content)
    {
        DebugConsole.Instance.LogDebug($"Creating annotation for {componentName}");
        //Create null annotaion reference
        AnnotationJson annotationJson = null;
        //check the message type
        if (messageType == "Text")
        {
            DebugConsole.Instance.LogDebug($"created TextAnnotation for {componentName}");
            //create a new serialisble text annotation object and populate it with the params
            annotationJson = new TextAnnotationJson
            {
                ComponentName = componentName,
                Author = author,
                Timestamp = dateTime,
                Content = content
            };
        }
        else if (messageType == "Voice")
        {
            DebugConsole.Instance.LogDebug($"created VoiceAnnotation for {componentName}");
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
    /// The ERROR IS OCCURING HERE
    /// </summary>
    /// <param name="annotation"></param>
    private void addAnnotationToJson(AnnotationJson annotation)
    {
        DebugConsole.Instance.LogDebug($"Adding annoation to json for {annotation.ComponentName}");
        //grab the current selectable objects transform
        Transform currentSelection = SelectionManager.Instance.currentSelection.transform;
        //
        if (!currentSelection)
        {
            DebugConsole.Instance.LogError("cannot add annotation to json as there is no current selectable");
            return;
        }
        //add the annotation to the current selections annotation component's list
        AnnotationComponent annotationComponent = currentSelection.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("cannot add annotation to json as there is no annotation " +
                "component on the current selectable");
            return;
        }
        //add annotation 
        annotationComponent.Annotations.Add(annotation);
        //get the parent of the current object
        Transform currentSelectionParent = SelectionManager.Instance.getSelectionRootTransform();
        if (!currentSelectionParent)
        {
            DebugConsole.Instance.LogError($"Cannot add annotation as the theres no parent of the current" +
                $" selection ({currentSelection.name}) somehow?");
            return;
        }
        string fileName = $"{GlobalConstants.ANNOTATION_DIR}/{currentSelectionParent.name}/{currentSelectionParent.name}_Annotation.json";
        if (!File.Exists(fileName))
        {
            DebugConsole.Instance.LogError($"Cannot add annotation data as file:{fileName} cannot be found");
            return;
        }
        DebugConsole.Instance.LogDebug($"file exists:{fileName} to write an annotation for {annotation.ComponentName}");
        //get the json content
        string annotationJson = File.ReadAllText(fileName);
        DebugConsole.Instance.LogDebug($"loading file content:{fileName}");
        //get the deserialised json object from the content
        ModelAnnotationJson parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson);
        //update the json object to now include the new annotation
        updateAnnotationJson(currentSelectionParent, parentAnnotationJson, currentSelection.name, annotation);
        DebugConsole.Instance.LogDebug($"Attempting to write to json to {fileName} for component {annotation.ComponentName} under model {currentSelectionParent} ");
        //write to the json file
        writeJson(parentAnnotationJson, fileName);
    }

    // call whenever a component's highlight colour is updated. Updates the Json file accordingly
    public void updateAnnotationHighlightJson(string highlightColour)
    {
        // highlighted object will be currently selected
        string rootName = SelectionManager.Instance.getSelectionRootTransform().name;
        string parentJsonFileName = $"{GlobalConstants.ANNOTATION_DIR}/{rootName}/{rootName}_Annotation.json";
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

    private void updateAnnotationJson(Transform parent, ModelAnnotationJson parentJson, string targetName, AnnotationJson annotation)
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
            if (foundChild)
            {
                DebugConsole.Instance.LogDebug($"found the next child to search from parent:" +
                    $"{parent.name} to child {foundChild.name}");
                //recursively call this method to find where to put the annotation
                updateAnnotationJson(foundChild.transform, subcomponent, targetName, annotation);
            }
        }
        DebugConsole.Instance.LogError($"couldnt find found {targetName} when updating the parent modelannotation json ");
    }

    public void deleteAnnotation(AnnotationJson annotationData)
    {
        //check if we have a currentSelection
        if (!SelectionManager.Instance.currentSelection)
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
        if (!File.Exists(jsonPath))
        {
            DebugConsole.Instance.LogError($"couldnt find file:{jsonPath}");
            return;
        }
        //deserialise the json into the ModelAnnotation object
        ModelAnnotationJson rootJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(File.ReadAllText(jsonPath), settings);
        //search the root json for the currently selected component
        ModelAnnotationJson targetJson = findComponent(rootJson, SelectionManager.Instance.currentSelection.name);
        if (targetJson == null)
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
        for (int i = 0; i < currentSelection.Annotations.Count; i++)
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
        //update UIManager
        DataPanelManager.Instance.updateAnnotations(currentSelection);
    }

    private ModelAnnotationJson findComponent(ModelAnnotationJson currentModelJson, string targetName)
    {
        if (currentModelJson.Name == targetName)
        {
            return currentModelJson;
        }
        foreach (ModelAnnotationJson subcomponent in currentModelJson.Subcomponents)
        {
            ModelAnnotationJson result = findComponent(subcomponent, targetName);
            if (result != null)
                return result;
        }
        return null;
    }

    private void findComponent(Transform currentComponent, string targetName, Transform target)
    {
        //check the current components name
        if (currentComponent.name == targetName)
        {
            target = currentComponent;
            return;
        }
        //for each sub component
        for (int i = 0; i < currentComponent.childCount; i++)
        {
            Transform subcomponent = currentComponent.GetChild(i);
            if (!subcomponent)
                continue;
            findComponent(subcomponent, targetName, target);
        }

    }

    
}