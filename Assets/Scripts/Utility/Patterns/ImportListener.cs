using System;
using System.Collections.Generic;
using UnityEngine;

public interface ImportListener
{
    public void OnImportComplete(List<GameObject> gameObjects);
}
