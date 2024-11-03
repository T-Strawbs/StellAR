using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPane : MonoBehaviour
{
    [SerializeField] protected RectTransform contentHolder;

    [SerializeField] protected AnimationUI contentPrefab;
    [SerializeField] private bool isActive = false;
    [SerializeField] private Animation currentModelAnimation;

    [SerializeField] private List<AnimationUI> pooledlUI = new List<AnimationUI>();
    [SerializeField] private List<AnimationUI> activelUI = new List<AnimationUI>();
    
    [SerializeField] private RectTransform defaultElement;

    public Animation CurrentAnimation
    {
        get 
        {
            return currentModelAnimation;
        }
        set
        {
            currentModelAnimation = value;
        }
    }

    private void Awake()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(updateSelection);
    }

    public  void populateScrollPane()
    {
        DebugConsole.Instance.LogDebug("clearing anim pane");
        //clear our scroll pane
        clear();
        //check if the current animation exists
        if(!currentModelAnimation)
        {
            DebugConsole.Instance.LogWarning("the is no animation to populate from");
            defaultElement.gameObject.SetActive(true);
            return;
        }
        //get the animation clips from our current animation
        List<AnimationClip> animationClips = AnimationHandler.getAllAnimationClips(currentModelAnimation);
        //check if there are animations
        if(animationClips.Count < 1)
        {
            DebugConsole.Instance.LogDebug("current no of anims is 0");
            defaultElement.gameObject.SetActive(true);
            return;
        }
        DebugConsole.Instance.LogDebug("current no of anims is > 0");
        defaultElement.gameObject.SetActive(false);
        //for each animation in current animations
        foreach (AnimationClip clip in animationClips)
        {
            DebugConsole.Instance.LogDebug($"creating ui for {clip.name}");
            AnimationUI animationUI;
            //check if we have an animationUI free in our pooled list
            if (pooledlUI.Count < 1)
            {
                DebugConsole.Instance.LogDebug("we had to create a prefab");
                //instantiate prefab
                animationUI = Instantiate<AnimationUI>(contentPrefab);
                //set its parent and local transforms
                animationUI.transform.SetParent(contentHolder);
                animationUI.transform.localPosition = Vector3.zero;
                animationUI.transform.localRotation = Quaternion.identity;
                animationUI.transform.localScale = Vector3.one;
            }
            else
            {
                DebugConsole.Instance.LogDebug("the was a prefab in the pool");
                //grab the UI out of the pool
                animationUI = pooledlUI[pooledlUI.Count-1];
                //remove the ref in pool
                pooledlUI.RemoveAt(pooledlUI.Count - 1);
            }
            DebugConsole.Instance.LogDebug("activatign anim UI");
            //activate the animationUI
            animationUI.gameObject.SetActive(true);
            //populate its content
            animationUI.populateContent(currentModelAnimation, clip, clip.name);
            //add it to the active UI list
            activelUI.Add(animationUI);
            DebugConsole.Instance.LogDebug("we shouldve been successful");
        }
    }

    private void clear()
    {
        //for each activeUI in our active UI list
        for(int i = 0; i < activelUI.Count; i++)
        {
            //pool the object
            pooledlUI.Add(activelUI[i]);
            //deactivate it
            activelUI[i].gameObject.SetActive(false);
        }
        //clear the active ui list
        activelUI.Clear();
    }

    public void updateSelection(Transform selection)
    {

        //**right now we are populating when the animation handler gets updated. which we'll need to change next semester.
        //populateScrollPane();
    }

    private void toggleButtonState()
    {
        isActive = !isActive;
        if (isActive)
        {
            //enable the button to be pressed

            return;
        }
        //disable the button from being pressed

    }
}
