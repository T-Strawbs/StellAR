using Microsoft.MixedReality.GraphicsTools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OutlineManager : MonoBehaviour, SelectionSubcriber
{
    // keep track of current outline to disable when new outline is enabled
    private MeshOutlineHierarchy currentOutline;

    // on launch assign outline to all models
    void Start()
    {
        // subscribe to SelectionManager
        this.subscribe();

        foreach (Transform child in Config.Instance.AllModels)
        {
            recursivelyAddOutline(child);
        }
    }

    // add outline to all components of all objects
    void recursivelyAddOutline(Transform parent)
    {
        Material material = Resources.Load<Material>("MRTK_Outline_Blue");
        MeshOutlineHierarchy newOutline = parent.AddComponent<MeshOutlineHierarchy>();
        newOutline.enabled = false;
        newOutline.OutlineMaterial = material;
        foreach (Transform child in parent)
        {
            recursivelyAddOutline(child);
        }
    }

    public void subscribe()
    {
        SelectionManager.Instance.addSubscriber(this);
    }

    // whenever new object is selected disable prev outline and enable new outline
    public void updateSelection(Transform selection)
    {
        if(currentOutline)
        {
            currentOutline.enabled = false;
        }
        currentOutline = SelectionManager.currentSelection.GetComponent<MeshOutlineHierarchy>();
        currentOutline.enabled = true;
    }
}
