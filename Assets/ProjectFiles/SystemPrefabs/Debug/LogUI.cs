using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogUI : MonoBehaviour
{
    [SerializeField] private TMP_Text logNumber;
    [SerializeField] private TMP_Text logContent;

    public void initialise(int logNumber, string content, Color color)
    {
        //set the log number text
        this.logNumber.text = logNumber.ToString();
        //set the content text
        this.logContent.text = content;
        //set the text colours
        this.logContent.color = color;
        this.logNumber.color = color;
    }

}
