using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModelComponent
{
    public string name;
    public string metadata;
    public List<ModelComponent> subcomponents;

    public ModelComponent(string nameIn)
    {
        name = nameIn;
        metadata = "";
        subcomponents = new List<ModelComponent>();
    }
}
