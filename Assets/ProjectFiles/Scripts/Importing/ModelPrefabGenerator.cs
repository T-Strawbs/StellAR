#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Class for generating prefabs of models when the user places a model into the MODEL_DIR.
/// It's a type of AssetPostprocessor meaning that anytime a new asset is added into the Assets/
/// directory while in editor, Unity will call this scripts behaviour. Also occurs when
/// the editor finishes loading.
/// </summary>
[InitializeOnLoad]
public class ModelPrefabGenerator : AssetPostprocessor
{
    /// <summary>
    /// global constructor. Used when the editor is loading.
    /// </summary>
    static ModelPrefabGenerator()
    {
        setupAssetPaths();
        generatePrefabs(getModelPaths());
    }

    /// <summary>
    /// method for setting up the model and prefab directories incase 
    /// they are deleted.
    /// </summary>
    private static void setupAssetPaths()
    {
        //check if the model directory exists
        if (!Directory.Exists(GlobalConstants.MODEL_DIR))
        {
            //it doesnt so create it
            Directory.CreateDirectory(GlobalConstants.MODEL_DIR);
        }
        //check if the prefab Directory exists
        if (!Directory.Exists(GlobalConstants.PREFAB_DIR))
        {
            //it doesnt so create it
            Directory.CreateDirectory(GlobalConstants.PREFAB_DIR);
        }
    }
    /// <summary>
    /// method for returning a list of file paths for models of acceptable format.
    /// </summary>
    /// <returns></returns>
    private static List<string> getModelPaths()
    {
        List<string> modelPaths = new List<string>();

        //for each accepted model format
        foreach (string format in GlobalConstants.MODEL_FORMATS)
        {
            //get the path of the model
            modelPaths.AddRange(Directory.GetFiles(GlobalConstants.MODEL_DIR, $"*{format}"));
        }
        return modelPaths;
    }

    private static void generatePrefabs(List<string>modelPaths)
    {
        if(modelPaths.Count < 1)
            return;

        //grab a list of all the existing model prefabs 
        List<string> prefabNames = new List<string>(Directory.GetFiles(GlobalConstants.PREFAB_DIR, "*.prefab"));
        //drop the exstention for an upcomming comparason
        prefabNames = prefabNames.Select(path => Path.GetFileNameWithoutExtension(path)).ToList();

        //for each model in our model paths
        foreach (string modelPath in modelPaths)
        {
            //get the name of the model
            string modelName = Path.GetFileNameWithoutExtension(modelPath);
            //check if the model name is in our prefab names
            if (prefabNames.Contains(modelName))
                //we already have a prefab of the same name so contine -- change this if you want to overwrite the existing prefab
                continue;

            //Load the model from the assets
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                Debug.LogWarning($"Failed to load model at {modelPath}");
                continue;
            }

            //Generate the path for the new prefab
            string prefabPath = Path.Combine(GlobalConstants.PREFAB_DIR, modelName + ".prefab");
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            try
            {
                //Create the prefab and save it to the prefab path
                PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
                //Debug.Log($"Prefab created for {modelName} at {prefabPath}");
            }
            catch (Exception e)
            {
                Debug.Log($"Couldn't generate Prefab created for {modelName} at {prefabPath}\n{e}");
            }
        }
    }

    /// <summary>
    /// Method that generates a prefab when a model is imported into the MODEL_DIR and saves it in PREFAB_DIR.
    /// Invoked when a new asset is imported into the project.
    /// </summary>
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    /// <param name="didDomainReload"></param>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        setupAssetPaths();

        //create a list to store the model paths from the imported assets
        List<string>modelPaths = new List<string>();

        foreach(string assetPath in importedAssets)
        {
            //if the imported asset was imported to the MODEL_DIR and is in an accepted Model format
            if(assetPath.StartsWith(GlobalConstants.MODEL_DIR) && isValidFormat(assetPath))
            {
                //we add it to the model paths
                modelPaths.Add(assetPath);
            }
        }

        //generate prefabs
        generatePrefabs(modelPaths);

        //tell the annotation processor to initalise the annotation data for the prefabs
        //AnnotationProcessor.initialiseAnnotationData();

        //Refresh the AssetDatabase to show the new prefabs
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// method for checking if an asset's file format is accepted and therefore valid.
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static bool isValidFormat(string assetPath)
    {
        return GlobalConstants.MODEL_FORMATS.Contains(Path.GetExtension(assetPath).ToLower());
    }
}
#endif //UNITY_EDITOR