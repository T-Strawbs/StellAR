using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : Singleton<Config>
{
    public readonly static string resourcePath = $"{Application.dataPath}/StreamingAssets/";
}
