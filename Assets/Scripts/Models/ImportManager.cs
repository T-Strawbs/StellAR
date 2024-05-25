using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportManager : Singleton<ImportManager>
{
   public void importModel(Transform model)
    {
        DebugConsole.Instance.LogDebug($"Imported model:{model.name}");
    }
}
