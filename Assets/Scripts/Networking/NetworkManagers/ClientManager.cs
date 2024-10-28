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
        MessageBasedInteractable newLocalSelectionMessageBased = newLocalSelection.GetComponent<MessageBasedInteractable>();
        bool isNetworked = newLocalSelectionMessageBased != null;

        // set previous lookup data to int.MaxValue in case previousSelection is null, we can check this value and not search for the prev selection in Rpc call if this is the case
        int prevLocalSelectionKey = int.MaxValue;
        int prevLocalSelectionIndex = int.MaxValue;

        // add new outline if newLocalSelection is not the same as previous selection
        if (previousSelection == null || previousSelection.gameObject != newLocalSelection.gameObject)
        {
            // remove previous outline
            if(previousSelection != null)
            {
                Destroy(previousSelection);

                // if is networked get the lookup data to use in Rpc later to remove previous outline over the network
                if(isNetworked)
                {
                    // get lookup data for previous selection to remove outline
                    MessageBasedInteractable prevLocalSelectionMessageBased = previousSelection.GetComponent<MessageBasedInteractable>();
                    prevLocalSelectionKey = prevLocalSelectionMessageBased.lookupData.parentKey;
                    prevLocalSelectionIndex = prevLocalSelectionMessageBased.lookupData.objectIndex;
                }
            }

            // if selection is networked send outline across the network
            if (isNetworked)
            {
                // get lookup data for new selection to add outline
                int newLocalSelectionKey = newLocalSelectionMessageBased.lookupData.parentKey;
                int newLocalSelectionIndex = newLocalSelectionMessageBased.lookupData.objectIndex;

                applyColourOutlineRpc(newLocalSelectionKey, newLocalSelectionIndex, prevLocalSelectionKey, prevLocalSelectionIndex, NetworkManager.Singleton.LocalClientId);
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

    [Rpc(SendTo.ClientsAndHost)]
    private void applyColourOutlineRpc(int newSelectionParentKey, int newSelectionObjectIndex, int prevSelectionParentKey, int prevSelectionObjectIndex, ulong clientId)
    {
        // only perform colour outline if not the client that sent the request
        if(NetworkManager.Singleton.LocalClientId !=  clientId)
        {
            // if previous selection wasn't null remove previous outline
            if (prevSelectionParentKey != int.MaxValue)
            {
                MessageBasedInteractable prevSelection = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(prevSelectionParentKey, prevSelectionObjectIndex);
                Destroy(prevSelection.GetComponent<BaseMeshOutline>());
            }

            // apply outline to new selection
            Transform applyColourToThis = MessageBasedInstanceManager.Instance.lookupNetworkInteractable(newSelectionParentKey, newSelectionObjectIndex).transform;

            // if new selection has no children it needs MeshOutline, else it needs MeshOutlineHierarchy
            if (applyColourToThis.childCount == 0)
            {
                if (applyColourToThis.GetComponent<Renderer>() != null)
                {
                    // add new outline
                    MeshOutline outline = applyColourToThis.AddComponent<MeshOutline>();

                    // set colour and width
                    outline.OutlineMaterial = outlineMaterials[(int)clientId % outlineMaterials.Count];
                    outline.OutlineWidth = 0.1f;

                    // update previous selection reference
                    previousSelection = outline;
                }
            }
            else
            {
                MeshOutlineHierarchy outline = applyColourToThis.AddComponent<MeshOutlineHierarchy>();
                outline.OutlineMaterial = outlineMaterials[(int)clientId % outlineMaterials.Count];
                outline.OutlineWidth = 0.1f;
                outline.ApplyOutlineMaterial();
                previousSelection = outline;
            }

        }
    }
}
