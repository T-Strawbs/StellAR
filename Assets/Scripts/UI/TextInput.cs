using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using UnityEngine;

public class TextInput : MonoBehaviour, IAnnotationInput
{
    [SerializeField] private PressableButton inputFieldBtn;
    [SerializeField] private PressableButton postBtn;

    private void Start()
    {

        postBtn.OnClicked.AddListener(postAnnotation);

    }

    public void postAnnotation ()
    {

    }

    private void invokeKeyboard()
    {
        Debug.Log("we would invoke the keyboard here");
    }


}
