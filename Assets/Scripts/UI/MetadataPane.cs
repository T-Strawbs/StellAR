using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MetadataPane : MonoBehaviour
{
    [SerializeField] private TMP_Text metadataContent;

    public void updateMetadataContent(string metadata)
    {
       metadataContent.text = metadata;
    }
}
