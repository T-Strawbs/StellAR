using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportManager : Singleton<ImportManager>
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float spawnDistance = 5f;

    public void importModel(Transform model)
    {
        if(!model)
        {
            DebugConsole.Instance.LogError($"somehow we are trying to import a model that doesnt exist");
            return;
        }
        //check if the models already active in which case for now dont execute the code again
        if(model.gameObject.activeInHierarchy)
        {
            DebugConsole.Instance.LogError($"{model.name} is already in the scene");
            return;
        }
        DebugConsole.Instance.LogDebug($"Importing model:{model.name}");
        //get the tranform of our camera which is our users pov origin
        Transform userTransform = mainCamera.transform;
        //calculate the models "spawn" position to be in front of the users view * spawnDistance
        Vector3 spawnPosition = userTransform.position + userTransform.forward * spawnDistance;
        //set the models spawn pos
        model.transform.position = spawnPosition;

        //active the model
        model.gameObject.SetActive(true);
        //SelectionManager.Instance.setSelection(model.GetComponent<Explodable>());
    }
}
