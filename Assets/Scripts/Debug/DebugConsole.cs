using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugConsole : Singleton<DebugConsole>
{
    [SerializeField] private TMP_Text currentSelectionText;
    [SerializeField] private TMP_Text previousSelectionText;
    [SerializeField] private string currentSelectionName;
    [SerializeField] private string previousSelectionName;
    [SerializeField] private RectTransform contentHolder;
    [SerializeField] private LogUI prefab;
    [SerializeField] private List<LogUI> logs;

    private void Start()
    {
        logs = new List<LogUI>();
        //set the default prev selection text
        previousSelectionName = "None";
        previousSelectionText.text = $"Previous Selection:{previousSelectionName}";
        //set the default current selection text
        currentSelectionName = "None";
        currentSelectionText.text = $"Current Selection:{currentSelectionName}";
        //make sure that we can post native unity logs into the debug console
        Application.logMessageReceived += handleUnityLog;
    }

    public void LogDebug(string log)
    {
        //instanitate the log prefab
        LogUI logUI = Instantiate<LogUI>(prefab);
        //initialise the log 
        logUI.initialise(logs.Count + 1, log, Color.white);
        //setup the parent and local transforms
        setParent(logUI);
        //add the log to the list
        logs.Add(logUI);
        //run a unity debug call
        Debug.Log(log);
    }

    public void LogError(string log)
    {
        //instanitate the log prefab
        LogUI logUI = Instantiate<LogUI>(prefab);
        //initialise the log 
        logUI.initialise(logs.Count + 1, log, Color.red);
        //setup the parent and local transforms
        setParent(logUI);
        //add the log to the list
        logs.Add(logUI);
        //run a unity debug call
        Debug.LogError(log);
    }

    public void LogWarning(string log)
    {
        //instanitate the log prefab
        LogUI logUI = Instantiate<LogUI>(prefab);
        //initialise the log 
        logUI.initialise(logs.Count + 1, log, Color.yellow);
        //setup the parent and local transforms
        setParent(logUI);
        //add the log to the list
        logs.Add(logUI);
        //run a unity debug call
        Debug.LogWarning(log);
    }

    public void updateSelection(string selectionName)
    {
        //set the previous selection name as the current selection name
        previousSelectionName = currentSelectionName;
        previousSelectionText.text = $"Previous Selection:{previousSelectionName}";
        //set the current text to the selection name
        currentSelectionName = selectionName;
        currentSelectionText.text = $"Current Selection: {currentSelectionName}";
    }

    private void setParent(LogUI logUI)
    {
        //set the log as a child of the content holder
        logUI.transform.SetParent(contentHolder);
        //set the local transforms of log ui
        logUI.transform.localPosition = Vector3.zero;
        logUI.transform.localRotation = Quaternion.identity;
        logUI.transform.localScale = Vector3.one;
    }

    private void handleUnityLog(string logString, string exceptionMsg, LogType logType)
    {
        //check if the log is an exception
        if (logType != LogType.Exception)
            return;
        //print the exception to the debug console
        LogError($"{logString}\n{exceptionMsg}");

    }
}
