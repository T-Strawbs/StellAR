using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScrollPane : MonoBehaviour
{
    [SerializeField] protected RectTransform contentHolder;
    
    public abstract void populateScrollPane();
}
