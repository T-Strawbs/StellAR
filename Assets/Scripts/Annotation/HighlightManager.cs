using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    [SerializeField]
    private PressableButton noneButton;
    [SerializeField]
    private PressableButton redButton;
    [SerializeField]
    private PressableButton greenButton;
    [SerializeField]
    private PressableButton blueButton;
    [SerializeField]
    private PressableButton yellowButton;

    private void Awake()
    {
        noneButton.OnClicked.AddListener(() => setHighlight("None"));
        redButton.OnClicked.AddListener(() => setHighlight("Red"));
        greenButton.OnClicked.AddListener(() => setHighlight("Green"));
        blueButton.OnClicked.AddListener(() => setHighlight("Blue"));
        yellowButton.OnClicked.AddListener(() => setHighlight("Yellow"));

    }

    public void setHighlight(string colour)
    {
        if (!SelectionManager.Instance.currentSelection)
        {
            DebugConsole.Instance.LogError("Cannot set highlight as there is no current selection");
            return;
        }
        DebugConsole.Instance.LogDebug($"setting highlight {SelectionManager.Instance.currentSelection.name} to {colour}");
        SelectionManager.Instance.currentSelection.GetComponent<AnnotationComponent>().changeHighlightColour(colour);
    }
}