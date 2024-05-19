using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;

public class AnnotationManager : MonoBehaviour
{

    [SerializeField] private List<Transform> models;
    [SerializeField] string jsonDirPath;
    private void Awake()
    {
        jsonDirPath = $"{Application.persistentDataPath}/";
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
            AnnotationJson parentAnnotationJson = new AnnotationJson(modelRoot.name);

            //check if the modelname_Annotation.json exists
            string jsonPath = $"{jsonDirPath}{modelRoot.name}_Annotation.json";
            if (File.Exists(jsonPath))
            {
                //json exists so we load annotation data from the json file
                string annotationJson = File.ReadAllText(jsonPath);
                //deserialise the data from json into usable objects
                parentAnnotationJson = JsonConvert.DeserializeObject<AnnotationJson>(annotationJson);
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

    private void populateAnnotationDataFromJson(Transform parentTransform,AnnotationJson parentComponent)
    {
        //create a new annotation component for the parent
        AnnotationComponent annotationComponent = new AnnotationComponent();
        //add the annotation data to the annotation component
        annotationComponent.Annotations = parentComponent.Annotations;
        //add annotation component to parent transform
        parentTransform.AddComponent<AnnotationComponent>();

        //get list of subcomponent transforms
        List<Transform> subcomponentTransforms = new List<Transform>();
        foreach(Transform subcomponent in parentTransform)
        {
            subcomponentTransforms.Add(subcomponent);
        }

        //-- removing dead json links

        //get a list of the parentComponent's subcomponents
        List<AnnotationJson> subcomponents = new List<AnnotationJson>(parentComponent.Subcomponents);
        //for each subcompnent in the parents subcomponents
        foreach(AnnotationJson subcomponent in subcomponents)
        {
            //find the c
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
            leftoverSubcomponent.gameObject.AddComponent<AnnotationComponent>();
            AnnotationJson subcomponent = new AnnotationJson(leftoverSubcomponent.name);
            parentComponent.Subcomponents.Add(subcomponent);

            // check and add children of this component
            createAnnotationJson(leftoverSubcomponent, subcomponent);
        }

    }

    private void createAnnotationJson(Transform parentTransform,AnnotationJson parentComponent)
    {
        //add a Annotation component to the parent
        parentTransform.AddComponent<AnnotationComponent>();
        //for each child of the parent
        foreach (Transform childTransform in parentTransform)
        {
            //create an empty Serialised Annotation object
            AnnotationJson childAnnotationJson = new AnnotationJson(childTransform.name);
            //add the Serialised annotation as a subcomponent of the parent
            parentComponent.Subcomponents.Add(childAnnotationJson);
            //recursively call for this childs decendants 
            createAnnotationJson(childTransform, childAnnotationJson);
        }
    }

    private void writeJson(AnnotationJson serialisedAnnotation, string jsonPath)
    {
        //write out annotation data to json
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(serialisedAnnotation,Formatting.Indented));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
