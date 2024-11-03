using Microsoft.MixedReality.GraphicsTools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OutlineManager : MonoBehaviour, NewSelectionListener
{
    // keep track of current outline to disable when new outline is enabled
    private MeshOutlineHierarchy currentOutline;

    private void Awake()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(onNewSelectionListener);
    }

    // on launch assign outline to all models
    void Start()
    {
        /*
        // subscribe to SelectionManager
        foreach (Transform child in Config.Instance.AllModels)
        {
            recursivelyAddOutline(child);
        }
        */
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

    // whenever new object is selected disable prev outline and enable new outline
    public void onNewSelectionListener(Transform selection)
    {
        if (currentOutline)
        {
            currentOutline.enabled = false;
        }
        currentOutline = SelectionManager.Instance.currentSelection.GetComponent<MeshOutlineHierarchy>();
        currentOutline.enabled = true;
    }
}
