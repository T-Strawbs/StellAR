using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class Config : Singleton<Config>
{
    public static string resourcePath;
    [SerializeField] private Transform allModels;
    [SerializeField] private string[] micNames;
    public static string micName;
    [SerializeField] private PressableButton btnPrefab;
    [SerializeField] private Transform btnPlate;

    [SerializeField] private Transform debugConsole;
    [SerializeField] private bool consoleIsActive;
    public static string timeFormat = "HH:mm:ss dd/MM/yyyy";

    private void Awake()
    {
        resourcePath = $"{Application.persistentDataPath}/";

        if (!consoleIsActive)
        {
            debugConsole.gameObject.SetActive(false);
            micName = Microphone.devices[0];
        }
        else
        {
            micNames = Microphone.devices;
        }
        

        
    }

    private void Start()
    {
        setupMicButtons();
    }

    private void setupMicButtons()
    {
        //foreach mic name in device names
        foreach(string name in Microphone.devices)
        {
            //instantiate btn
            PressableButton micBtn = Instantiate<PressableButton>(btnPrefab);
            TMP_Text text = micBtn.GetComponentInChildren<TMP_Text>();
            if(text)
            {
                text.enabled = true;
                text.text = name;
            }
                
            //set callback event
            micBtn.OnClicked.AddListener(() => setMicName(name));
            //set the parent of the btn to the btn plate
            micBtn.transform.SetParent(btnPlate);
            //set the btns local transforms
            micBtn.transform.localPosition = Vector3.zero;
            micBtn.transform.localRotation = Quaternion.identity;
            micBtn.transform.localScale = Vector3.one;
        }
    }

    private void setMicName(string name)
    {
        micName = name;
    }

    public ref Transform AllModels
    {
        get { return ref allModels; }
    } 


}
