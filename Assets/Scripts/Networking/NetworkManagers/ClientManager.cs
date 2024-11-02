using Microsoft.MixedReality.GraphicsTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : NetworkSingleton<ClientManager>
{
    public List<Material> outlineMaterials = new List<Material>();
    private BaseMeshOutline previousSelection;

    // add listener for when new object is selected
    private void Start()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(applyOutlineToNewSelection);
    }

    /// <summary>
    /// Remove outline, called from within ClientManager.
    /// </summary>
    /// <param name="removeOutlineFromThis">Object you want to remove outline from.</param>
    private void removeOutline(BaseMeshOutline outline)
    {
        // check that input object has an outline to remove
        if (outline != null)
        {
            Destroy(outline);
        }
        else
        {
            DebugConsole.Instance.LogError("Attmepted to remove outline from object with no outline.");
        }
    }

    /// <summary>
    /// Sent across the network to remove the outline from the input object.
    /// </summary>
    /// <param name="parentKey"></param>
    /// <param name="objectIndex"></param>
    [Rpc(SendTo.NotMe)]
    private void removeOutlineRpc(int parentKey, int objectIndex)
    {
        MessageBasedInteractable removeOutlineFromThis = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(parentKey, objectIndex);
        BaseMeshOutline outline = removeOutlineFromThis.GetComponent<BaseMeshOutline>();
        removeOutline(outline);
    }

    /// <summary>
    /// Remove outline, send across network.
    /// </summary>
    /// <param name="removeOutlineFromThis">Object you want to remove outline from.</param>
    public void removeOutline(Transform removeOutlineFromThis)
    {
        // check that a null transform wasn't input
        if(removeOutlineFromThis != null)
        {
            BaseMeshOutline outline = removeOutlineFromThis.GetComponent<BaseMeshOutline>();
            if(outline != null)
            {
                removeOutline(outline);

                // transmit outline removal across the network
                if (ApplicationManager.Instance.isOnline())
                {
                    MessageBasedInteractable networkedObject = removeOutlineFromThis.GetComponent<MessageBasedInteractable>();
                    if (networkedObject != null)
                    {
                        removeOutlineRpc(networkedObject.lookupData.parentKey, networkedObject.lookupData.objectIndex);
                    }
                    else
                    {
                        DebugConsole.Instance.LogWarning("Attempted to transmit outline removal across network but object was not networked.");
                    }
                }
            }
            else
            {
                DebugConsole.Instance.LogWarning("Attempted to remove outline from object with no outline.");
            }
        }
        else
        {
            DebugConsole.Instance.LogError("Attempted to remove outline from null object.");
        }
    }

    /// <summary>
    /// Add outline, send across network.
    /// </summary>
    /// <param name="addOutlineToThis"></param>
    public void addOutline(Transform addOutlineToThis)
    {
        // ensure object exists
        if (addOutlineToThis != null)
        {
            // set client id, if offline use 0
            int clientId = ApplicationManager.Instance.isOnline() ? (int)NetworkManager.LocalClientId : 0;

            _addOutline(addOutlineToThis, clientId);
            // if online send new outline across network
            if (ApplicationManager.Instance.isOnline())
            {
                MessageBasedInteractable networkedObject = addOutlineToThis.GetComponent<MessageBasedInteractable>();
                if (networkedObject != null)
                {
                    addNewOutlineRpc(networkedObject.lookupData.parentKey, networkedObject.lookupData.objectIndex, (int)NetworkManager.Singleton.LocalClientId);
                }
            }
        }
    }

    /// <summary>
    /// Add outline, called within Client Manager.
    /// </summary>
    /// <param name="addOutlineToThis"></param>
    /// <param name="colourToUse"></param>
    private void _addOutline(Transform addOutlineToThis, int colourToUse)
    {
        // ensure input object exists
        if (addOutlineToThis != null)
        {
            // if new selection has no children it needs MeshOutline, else it needs MeshOutlineHierarchy
            if (addOutlineToThis.childCount == 0)
            {
                // ensure it has a renderer or outline won't work
                if (addOutlineToThis.GetComponent<Renderer>() != null)
                {
                    MeshOutline outline = addOutlineToThis.GetComponent<MeshOutline>();

                    // if object doesn't already have an outline add one
                    if (outline == null)
                    {
                        outline = addOutlineToThis.AddComponent<MeshOutline>();

                    }

                    // set colour and width
                    outline.OutlineMaterial = outlineMaterials[colourToUse % outlineMaterials.Count];
                    outline.OutlineWidth = 0.1f;
                }
                else
                {
                    DebugConsole.Instance.LogError("Attempted to add outline to object with no renderer.");
                }
            }
            else
            {
                MeshOutlineHierarchy outline = addOutlineToThis.GetComponent<MeshOutlineHierarchy>();

                //if object doesn't have an outline yet add one
                if (outline == null)
                {
                    outline = addOutlineToThis.AddComponent<MeshOutlineHierarchy>();
                }

                // set colour and width
                outline.OutlineMaterial = outlineMaterials[colourToUse % outlineMaterials.Count];
                outline.OutlineWidth = 0.1f;
                outline.ApplyOutlineMaterial();
            }
        }
        else
        {
            DebugConsole.Instance.LogWarning("Attempted to remove outline from null object.");
        }
    }

    [Rpc(SendTo.NotMe)]
    private void addNewOutlineRpc(int parentKey, int objectIndex, int senderId)
    {
        MessageBasedInteractable addOutlineToThis = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(parentKey, objectIndex);
        _addOutline(addOutlineToThis.transform, senderId);
    }

    /// <summary>
    /// Used to apply outline when a new object is selected.
    /// </summary>
    /// <param name="newLocalSelection"></param>
    private void applyOutlineToNewSelection(Transform newLocalSelection)
    {
        // set client id, if offline use 0
        int clientId = ApplicationManager.Instance.isOnline() ? (int)NetworkManager.LocalClientId : 0;

        BaseMeshOutline outline = newLocalSelection.GetComponent<BaseMeshOutline>();

        // new selection doesn't have an outline or has someone else's outline
        bool hasDifferentOutline = (outline == null || outline.OutlineMaterial != outlineMaterials[clientId % outlineMaterials.Count]);

        // only update outline if new selection is different from prev selection or it doesn't have my outline
        if (previousSelection == null || newLocalSelection != previousSelection.transform || hasDifferentOutline)
        {
            // if prev selection exists and has my outline colour, remove it
            if (previousSelection != null && previousSelection.OutlineMaterial == outlineMaterials[clientId % outlineMaterials.Count])
            {
                removeOutline(previousSelection.transform);
            }

            addOutline(newLocalSelection);
            previousSelection = newLocalSelection.GetComponent<BaseMeshOutline>();
        }
    }
}
