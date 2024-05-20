using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MetadataJson
{
    public string name;
    public string metadata;
    public List<MetadataJson> subcomponents;

    public MetadataJson(string nameIn)
    {
        name = nameIn;
        metadata = "";
        subcomponents = new List<MetadataJson>();
    }
}
