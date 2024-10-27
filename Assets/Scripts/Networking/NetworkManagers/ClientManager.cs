using Microsoft.MixedReality.GraphicsTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : NetworkSingleton<ClientManager>
{
    private int _clientId = 0;
    public List<Material> outlineMaterials = new List<Material>();
    private BaseMeshOutline previousSelection;

    // add listener for when new object is selected
    private void Start()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(applyColourOutline);
    }

    public void setClientId(int clientId)
    {
        _clientId = clientId;
    }

    // apply outline to newly selected object if it is a new object
    public void applyColourOutline(Transform newLocalSelection)
    {
        if(previousSelection == null || previousSelection.gameObject != newLocalSelection.gameObject)
        {
            // remove previous outline
            if(previousSelection != null)
            {
                Destroy(previousSelection);
            }

            // if new selection has no children it needs MeshOutline, else it needs MeshOutlineHierarchy
            if (newLocalSelection.childCount == 0)
            {
                if(newLocalSelection.GetComponent<Renderer>() != null)
                {
                    // add new outline
                    MeshOutline outline = newLocalSelection.AddComponent<MeshOutline>();

                    // set colour and width
                    outline.OutlineMaterial = outlineMaterials[_clientId % outlineMaterials.Count];
                    outline.OutlineWidth = 0.1f;

                    // update previous selection reference
                    previousSelection = outline;
                }
            }
            else
            {
                MeshOutlineHierarchy outline = newLocalSelection.AddComponent<MeshOutlineHierarchy>();
                outline.OutlineMaterial = outlineMaterials[_clientId % outlineMaterials.Count];
                outline.OutlineWidth = 0.1f;
                outline.ApplyOutlineMaterial();
                previousSelection = outline;
            }
        }
    }
}
