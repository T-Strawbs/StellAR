using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : Singleton<ClientManager>
{
    private Color clientColour = Color.blue;

    private void Start()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(applyColourOutline);
    }

    public void setClientColour(Color newClientColour)
    {
        clientColour = newClientColour;
    }

    public Color getClientColour()
    {
        return clientColour;
    }

    public void applyColourOutline(Transform newLocalSelection)
    {

    }
}
