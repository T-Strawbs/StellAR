
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using Unity.XR.CoreUtils;
using UnityEngine;

public class AnnotationManager : NetworkSingleton<AnnotationManager>, PrefabInstantationListener, StartupProcess
{
    //this allows us to ensure that the annotation json subtypes are accepted
    private JsonSerializerSettings settings;
    private Dictionary<int, ModelAnnotationJson> modelAnnotationJsons = new Dictionary<int, ModelAnnotationJson>();

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
        ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
    }

    public void onStartupProcess()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            if (NetworkManager.Singleton.IsServer)
                registerMessages();
        };
        //Register lambda event that executes when the a client connects to the server.
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) =>
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId == clientID)
                registerMessages();
        };
    }

    public void registerMessages()
    {
        //custom messaging function for sending audio data from client to server
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("postAudioAnnotationRpc", postAudioAnnotationRpc);
    }

    /// <summary>
    /// Called when prefabs are instantiated
    /// </summary>
    /// <param name="instance">Newly instantiated object</param>
    public void onPrefabInstantiation(GameObject instance)
    {
        // if application is in offline mode initialise annotation data normally
        if(!ApplicationManager.Instance.isOnline())
        {
            initialiseAnnotations(instance);
        }
        else
        {
            // if application is online and client is hosting, initialise annotation data normally then send to clients over the network
            if(NetworkManager.Singleton.IsHost)
            {
                initialiseAnnotations(instance);
            }
            else
            {
                // else you are a client, and you need to request annotation data from server
                requestAnnotationDataFromServerRpc(NetworkManager.Singleton.LocalClientId, instance.GetComponent<MessageBasedInteractable>().lookupData.parentKey);
            }
        }
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
            //Populate The model's GO with the Json Data
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

        // if prefab is a network prefab add the created ModelAnnotationJson to dictionary for lookup when requested by clients via Rcp
        MessageBasedInteractable messageBasedInteractable = prefabInstance.GetComponent<MessageBasedInteractable>();
        if(messageBasedInteractable != null)
        {
            modelAnnotationJsons.Add(messageBasedInteractable.lookupData.parentKey, parentAnnotationJson);
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

    /// <summary>
    /// Creates an AnnotationJson from input data, and calls addAnnotationToJson() which adds annotation to object and writes the new annotation to JSON on disk.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="messageType"></param>
    /// <param name="author"></param>
    /// <param name="dateTime"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public AnnotationJson createAnnotationJson(Transform component, string messageType, string author, string dateTime, string content)
    {
        DebugConsole.Instance.LogDebug($"Creating annotation for {component.name}");
        //Create null annotaion reference
        AnnotationJson annotationJson = null;
        //check the message type
        if (messageType == "Text")
        {
            DebugConsole.Instance.LogDebug($"created TextAnnotation for {component.name}");
            //create a new serialisble text annotation object and populate it with the params
            annotationJson = new TextAnnotationJson(component.name, author, dateTime, content);
        }
        else if (messageType == "Voice")
        {
            DebugConsole.Instance.LogDebug($"created VoiceAnnotation for {component.name}");
            //create a new serialisble voice annotation object and populate it with the params
            annotationJson = new VoiceAnnotationJson(component.name, author, dateTime, content);
        }
        else
        {
            DebugConsole.Instance.LogError($"Cant create annotation for {component.name} " +
                $"as the message type {messageType} is invalid");
            return null;
        }
        //write to json and add annotation to the AnnotationComponent
        addAnnotationToJson(annotationJson, component);
        return annotationJson;
    }

    /// <summary>
    /// Add input annotation object to the root parent model's Json in memory and on disk
    /// </summary>
    /// <param name="annotation"></param>
    private void addAnnotationToJson(AnnotationJson annotation, Transform component)
    {
        DebugConsole.Instance.LogDebug($"Adding annoation to json for {annotation.ComponentName}");

        //add the annotation to the current selections annotation component's list
        AnnotationComponent annotationComponent = component.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("cannot add annotation to json as there is no annotation " +
                "component on the current selectable");
            return;
        }
        //add annotation 
        annotationComponent.Annotations.Add(annotation);
        //get the parent of the current object
        Transform componentParent = SelectionManager.Instance.getModelRoot(component);
        if (!componentParent)
        {
            DebugConsole.Instance.LogError($"Cannot add annotation as the theres no parent of the current" +
                $" selection ({componentParent.name}) somehow?");
            return;
        }
        string fileName = $"{GlobalConstants.ANNOTATION_DIR}/{componentParent.name}/{componentParent.name}_Annotation.json";
        if (!File.Exists(fileName))
        {
            DebugConsole.Instance.LogError($"Cannot add annotation data as file:{fileName} cannot be found");
            return;
        }
        DebugConsole.Instance.LogDebug($"file exists:{fileName} to write an annotation for {annotation.ComponentName}");
        //load json file
        string annotationJson = File.ReadAllText(fileName);
        DebugConsole.Instance.LogDebug($"loading file content:{fileName}");
        //deserialised json into memory
        ModelAnnotationJson parentAnnotationJson = JsonConvert.DeserializeObject<ModelAnnotationJson>(annotationJson);
        //update the json object in memory to now include the new annotation
        updateAnnotationJson(componentParent, parentAnnotationJson, component.name, annotation);
        DebugConsole.Instance.LogDebug($"Attempting to write to json to {fileName} for component {annotation.ComponentName} under model {componentParent} ");
        //write json object from memory to the json file on disk
        writeJson(parentAnnotationJson, fileName);
    }

    // call whenever a component's highlight colour is updated. Updates the Json file accordingly
    public void updateAnnotationHighlightJson(Transform objectToUpdate, string highlightColour)
    {
        // highlighted object will be currently selected
        string rootName = SelectionManager.Instance.getModelRoot(objectToUpdate).name;
        string parentJsonFileName = $"{GlobalConstants.ANNOTATION_DIR}/{rootName}/{rootName}_Annotation.json";
        string targetName = objectToUpdate.name;
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

    /// <summary>
    /// Depth-first search of a model's JSON tree in memory (ModelAnnotationJson) to find the target component and add an annotation to it.
    /// Used to keep the annotations on disk (ModelAnnotationJson) in sync with annotation changes made to during runtime (AnnotationComponent).
    /// </summary>
    /// <param name="current"></param>
    /// <param name="currentJson"></param>
    /// <param name="targetName"></param>
    /// <param name="annotation"></param>
    private void updateAnnotationJson(Transform current, ModelAnnotationJson currentJson, string targetName, AnnotationJson annotation)
    {
        DebugConsole.Instance.LogDebug($"updating json attempting to find the targetname:{targetName}" +
            $" inside {current.name}");

        //check if the current name matches the target name
        if (currentJson.Name == targetName)
        {
            DebugConsole.Instance.LogDebug($"found {targetName} so we are adding the annotation");
            //add the annotation to the subcomponent's annotations
            currentJson.Annotations.Add(annotation);
            return;
        }

        // run the search for the target in all levels of the model 
        foreach (ModelAnnotationJson subcomponent in currentJson.Subcomponents)
        {
            // get the children of the current component and run the search for the target on them
            Interactable currentInteractable = current.GetComponent<Interactable>();
            GameObject foundChild = currentInteractable.findNamedChildDirect(subcomponent.Name);
            if (foundChild)
            {
                DebugConsole.Instance.LogDebug($"found the next child to search from parent:" +
                    $"{current.name} to child {foundChild.name}");
                //recursively call this method to find where to put the annotation
                updateAnnotationJson(foundChild.transform, subcomponent, targetName, annotation);
            }
        }
        DebugConsole.Instance.LogError($"couldnt find found {targetName} when updating the parent modelannotation json ");
    }

    /// <summary>
    /// Deletes input annotation data from input component on the JSON that is on the disk, then calls deleteAnnotationFromRuntime() to remove it from the Game Object
    /// </summary>
    /// <param name="annotationToDelete"></param>
    /// <param name="deleteAnnotationFromThis"></param>
    public void deleteAnnotationFromDisk(AnnotationJson annotationToDelete, Transform deleteAnnotationFromThis)
    {
        //find the name of the component's root predecessor
        string rootName = SelectionManager.Instance.getModelRoot(deleteAnnotationFromThis.transform).name;
        if (rootName == "")
        {
            DebugConsole.Instance.LogError($"Cannot find root predecessor of {deleteAnnotationFromThis.name}");
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
        ModelAnnotationJson targetJson = findComponent(rootJson, deleteAnnotationFromThis.name);
        if (targetJson == null)
        {
            DebugConsole.Instance.LogError($"Couldnt find {deleteAnnotationFromThis.name} in {rootJson.Name}");
            return;
        }
        DebugConsole.Instance.LogDebug($"this annotation comps anno count is{targetJson.Annotations.Count}");

        //find the target annotation in the target json's annotations
        for (int i = 0; i < targetJson.Annotations.Count; i++)
        {
            DebugConsole.Instance.LogDebug($"checking if (" +
                $"{targetJson.Annotations[i].ComponentName},{targetJson.Annotations[i].Author},{targetJson.Annotations[i].Timestamp})" +
                $" is equal to " +
                $"({annotationToDelete.ComponentName},{annotationToDelete.Author},{annotationToDelete.Timestamp})");
            if (targetJson.Annotations[i].Equals(annotationToDelete))
            {
                DebugConsole.Instance.LogDebug("The annotations match in current selection");
                targetJson.Annotations.RemoveAt(i);
                break;
            }
        }

        //update json
        writeJson(rootJson, jsonPath);

        // delete annotation from the GameObject
        deleteAnnotationFromRuntime(annotationToDelete, deleteAnnotationFromThis);

        // if annotation was voice annotation, delete the audio file from system
        if(annotationToDelete.MessageType == GlobalConstants.VOICE_ANNOTATION)
        {
            File.Delete(annotationToDelete.Content);
        }
    }

    /// <summary>
    /// Deletes the input annotation from the input GameObject.
    /// </summary>
    /// <param name="deleteAnnotationFromThis"></param>
    /// <param name="annotationToDelete"></param>
    private void deleteAnnotationFromRuntime(AnnotationJson annotationToDelete, Transform deleteAnnotationFromThis)
    {
        //remove the annotation from the component's AnnotationComponent
        AnnotationComponent currentSelection = deleteAnnotationFromThis.GetComponent<AnnotationComponent>();
        if (!currentSelection)
        {
            DebugConsole.Instance.LogError($"Couldn't find {deleteAnnotationFromThis.name} annotation component");
            return;
        }

        for (int i = 0; i < currentSelection.Annotations.Count; i++)
        {
            if (currentSelection.Annotations[i].Equals(annotationToDelete))
            {
                DebugConsole.Instance.LogDebug("The annotations match in current selection");
                currentSelection.Annotations.RemoveAt(i);
                break;
            }
        }

        //update UIManager if annotation was deleted from current selection
        if(SelectionManager.Instance.currentSelection == deleteAnnotationFromThis.GetComponent<Interactable>())
        {
            DataPanelManager.Instance.updateAnnotations(currentSelection);
        }
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

    // executes on the server to return annotation data associated with the key
    [Rpc(SendTo.Server)]
    private void requestAnnotationDataFromServerRpc(ulong clientId, int modelParentKey)
    {
        NetworkModelAnnotationJson networkAnnotation = new NetworkModelAnnotationJson(modelAnnotationJsons[modelParentKey]);

        // send converted network model annotation data over the network to all clients
        sentAnnotationDataToClientRpc(modelParentKey, networkAnnotation, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    /// <summary>
    /// Called from the server/host in response to a client calling requestAnnotationDataFromServer() to send annotation data
    /// </summary>
    /// <param name="modelParentKey">The key used to identify which model the annotation data is for via MessageBasedInstanceManager.Instance.lookupNetworkInteractable()</param>
    /// <param name="networkAnnotationData">The annotation data used to populate the spawned prefab</param>
    /// <param name="rpcParams">Used to identify which client to send the response to</param>
    [Rpc(SendTo.SpecifiedInParams)]
    private void sentAnnotationDataToClientRpc(int modelParentKey, NetworkModelAnnotationJson networkAnnotationData, RpcParams rpcParams = default)
    {
        // get the model instance that the received annotation data is for, can enter 0 as object index as parent will always be index 0
        MessageBasedInteractable modelToPopulate = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(modelParentKey, 0);
        DebugConsole.Instance.LogDebug($"Found model with network lookup: {modelToPopulate.name}");

        // convert network annotation data to model annotation data and populate model with data
        ModelAnnotationJson modelAnnotation = new ModelAnnotationJson(networkAnnotationData);
        populateAnnotationDataFromJson(modelToPopulate.transform, modelAnnotation);
    }

    /// <summary>
    /// Post an annotation to the server. Creates an AnnotationJson based on input content and adds it to the AnnotationComponent of the associated GameObject.
    /// </summary>
    /// <param name="lookupData">The lookup data for the object the annotation is being placed on.</param>
    /// <param name="content">The annotation content.</param>
    /// <param name="annotationType">The type of annotation, from GlobalConstants.</param>
    [Rpc(SendTo.Server)]
    public void postAnnotationServerRpc(NetworkInteractableLookupData lookupData, string content, string annotationType)
    {
        //get the current date and time
        string currentDateTime = DateTime.Now.ToString(GlobalConstants.TIME_FORMAT);
        //get the object to add annotation to
        Transform addAnnotationToThis = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).transform;

        //tell Annotation manager to create annotation Json, this also adds it to the component and writes the annotation to disk.
        AnnotationJson annotation = createAnnotationJson(
            addAnnotationToThis,
            annotationType,
            "Default Author",// we need to replace this once we have multiple active users
            currentDateTime,
            content
        );

        //broadcast new annotation to clients
        NetworkAnnotationJson networkAnnotation = new NetworkAnnotationJson(annotation);
        broadcastNewAnnotationRpc(lookupData, networkAnnotation);

        //if annotation was for current selection update data pane
        AnnotationComponent annotationComponent = addAnnotationToThis.GetComponent<AnnotationComponent>();
        if (SelectionManager.Instance.currentSelection == addAnnotationToThis.GetComponent<Interactable>())
        {
            DataPanelManager.Instance.updateAnnotations(annotationComponent);
        }
    }

    /// <summary>
    /// Called from server when a new annotation is created. Adds the received annotation data to the corresponding component.
    /// </summary>
    /// <param name="networkAnnotation"></param>
    /// <param name="lookupData"></param>
    [Rpc(SendTo.NotServer)]
    public void broadcastNewAnnotationRpc(NetworkInteractableLookupData lookupData, NetworkAnnotationJson networkAnnotation)
    {
        //find the component the annotation needs to be added to
        Transform component = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).transform;
        if(component == null)
        {
            DebugConsole.Instance.LogError($"Recieved annotation data from server but the component doesn't exist on this device.");
            return;
        }

        //add the annotation to the component
        AnnotationComponent annotationComponent = component.GetComponent<AnnotationComponent>();
        if (!annotationComponent)
        {
            DebugConsole.Instance.LogError("cannot add annotation to json as there is no annotation " +
                "component on the component passed from the server");
            return;
        }
        
        if(networkAnnotation.MessageType == GlobalConstants.TEXT_ANNOTATION)
        {
            TextAnnotationJson annotation = new TextAnnotationJson(networkAnnotation);
            annotationComponent.Annotations.Add(annotation);
        }
        else if (networkAnnotation.MessageType == GlobalConstants.VOICE_ANNOTATION)
        {
            VoiceAnnotationJson annotation = new VoiceAnnotationJson(networkAnnotation);
            annotationComponent.Annotations.Add(annotation);
        }
        else
        {
            DebugConsole.Instance.LogError("Received annotation data from server but it was not a valid Annotation Message Type.");
            return;
        }

        //if the annotation was for the current selection update the data pane to show the new annotation
        if(SelectionManager.Instance.currentSelection == component.GetComponent<Interactable>())
        {
            DataPanelManager.Instance.updateAnnotations(annotationComponent);
        }

    }

    /// <summary>
    /// Called when someone on the network wants to delete an annotation.
    /// Is run on the server to delete the annotation from disk and runtime, and broadcasts to clients make them delete the annotation from their runtimes.
    /// </summary>
    /// <param name="lookupData">Lookup data to find the component the annotation belongs to.</param>
    /// <param name="networkAnnotation">Annotation to be deleted.</param>
    [Rpc(SendTo.Server)]
    public void deleteAnnotationServerRpc(NetworkInteractableLookupData lookupData, NetworkAnnotationJson networkAnnotation)
    {
        Transform deleteAnnotationFromThis = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).transform;
        if(networkAnnotation.MessageType == GlobalConstants.TEXT_ANNOTATION)
        {
            TextAnnotationJson annotation = new TextAnnotationJson(networkAnnotation);
            deleteAnnotationFromDisk(annotation, deleteAnnotationFromThis);
        }
        else if(networkAnnotation.MessageType == GlobalConstants.VOICE_ANNOTATION)
        {
            VoiceAnnotationJson annotation = new VoiceAnnotationJson(networkAnnotation);
            deleteAnnotationFromDisk(annotation, deleteAnnotationFromThis);
        }
        else
        {
            DebugConsole.Instance.LogError("Received request to delete annotation but Message Type was not valid.");
        }
        deleteAnnotationClientRpc(lookupData, networkAnnotation);
    }

    /// <summary>
    /// Called on clients by server/host when an annotation has been deleted. Removes the annotation from the GameObject.
    /// </summary>
    /// <param name="lookupData">Lookup data to find the component the annotation belongs to.</param>
    /// <param name="networkAnnotation">Annotation to be deleted.</param>
    [Rpc(SendTo.NotServer)]
    private void deleteAnnotationClientRpc(NetworkInteractableLookupData lookupData, NetworkAnnotationJson networkAnnotation)
    {
        Transform deleteAnnotationFromThis = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(lookupData.parentKey, lookupData.objectIndex).transform;
        if (networkAnnotation.MessageType == GlobalConstants.TEXT_ANNOTATION)
        {
            TextAnnotationJson annotation = new TextAnnotationJson(networkAnnotation);
            deleteAnnotationFromRuntime(annotation, deleteAnnotationFromThis);
        }
        else if (networkAnnotation.MessageType == GlobalConstants.VOICE_ANNOTATION)
        {
            VoiceAnnotationJson annotation = new VoiceAnnotationJson(networkAnnotation);
            deleteAnnotationFromRuntime(annotation, deleteAnnotationFromThis);
        }
        else
        {
            DebugConsole.Instance.LogError("Received request to delete annotation but Message Type was not valid.");
        }
    }

    /// <summary>
    /// Called on client to package audio annotation and send to server.
    /// </summary>
    /// <param name="lookupData">Lookup data for associated component.</param>
    /// <param name="networkAudioClip">Audio data of the audio annotation.</param>
    /// <param name="channels">Number of channels in the audio clip.</param>
    public void postAudioAnnotationServer(NetworkInteractableLookupData lookupData, float[] networkAudioClip,  int channels)
    {
        AudioAnnotationPostRequest postRequest = new AudioAnnotationPostRequest(lookupData, networkAudioClip, channels);

        // get size of data to be sent over network
        var writeSize = FastBufferWriter.GetWriteSize(postRequest.lookupData) +
            FastBufferWriter.GetWriteSize(postRequest.audioData) +
            FastBufferWriter.GetWriteSize(postRequest.numChannels);

        // send audio data to server
        var writer = new FastBufferWriter(writeSize, Allocator.Temp);
        using (writer)
        {
            DebugConsole.Instance.LogDebug("Sending audio data to server.");
            //write the audio request response to the writer
            writer.WriteValueSafe(postRequest);
            //send request to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "postAudioAnnotationRpc", NetworkManager.ServerClientId, writer, NetworkDelivery.ReliableFragmentedSequenced
                );
        }
    }

    /// <summary>
    /// Called from client to server to save an audio annotation to server disk, create an Annotation for it and broadcast the annotation to clients.
    /// </summary>
    /// <param name="lookupData">Lookup data for object associated with new voice Annotation.</param>
    /// <param name="networkAudioClip">Float array representing audio data.</param>
    /// <param name="numSamples">Length of audio clip in number of samples.</param>
    /// <param name="channels">Number of channels of audio clip.</param>
    /// <param name="frequency">Frequency of audio clip.</param>
    public void postAudioAnnotationRpc(ulong senderId, FastBufferReader messagePayload)
    {
        // get network data 
        AudioAnnotationPostRequest networkResponse;
        messagePayload.ReadValueSafe(out networkResponse);

        // convert raw audio data into AudioClip
        float[] audioData = networkResponse.audioData;
        int numSamples = audioData.Length / networkResponse.numChannels;

        //convert network data to AudioClip
        AudioClip clip = null;

        try
        {
            clip = AudioClip.Create("", numSamples, networkResponse.numChannels, GlobalConstants.SAMPLE_RATE, false);
            clip.SetData(audioData, 0);

            //get the current date and time to store in the annotation data
            string currentDateTime = DateTime.Now.ToString(GlobalConstants.TIME_FORMAT);
            //format the current datetime so that we can save a file without IO pointing a gun at us
            string dateTimeFormatted = currentDateTime.Replace(':', '-').Replace(' ', '-').Replace('/', '-');

            //create filename from the componet name + datetime
            Interactable addAnnotationToThis = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(networkResponse.lookupData.parentKey, networkResponse.lookupData.objectIndex);
            string fileName = $"{GlobalConstants.ANNOTATION_DIR}/{addAnnotationToThis.name}/{addAnnotationToThis.name}_{"DefaultAuthor"}_{dateTimeFormatted}";

            //save audio to file
            SavWav.Save(fileName, clip);

            //post filename on server and broadcast to clients
            AnnotationManager.Instance.postAnnotationServerRpc(networkResponse.lookupData, $"{fileName}.wav", GlobalConstants.VOICE_ANNOTATION);

            DebugConsole.Instance.LogDebug("we wouldve \"created\" a voice annotation");
        }
        catch (System.Exception e)
        {
            DebugConsole.Instance.LogError($"Unable to create audio clip using data retrieved from client.\n{e.ToString()}");
        }
    }
}

public struct AudioAnnotationPostRequest : INetworkSerializable
{
    public NetworkInteractableLookupData lookupData;
    public float[] audioData;
    public int numChannels;
    public AudioAnnotationPostRequest(NetworkInteractableLookupData lookupData, float[] audioData, int numChannels)
    {
        this.lookupData = lookupData;
        this.audioData = audioData;
        this.numChannels = numChannels;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref lookupData);
        serializer.SerializeValue(ref audioData);
        serializer.SerializeValue(ref numChannels);
    }
}