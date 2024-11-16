using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class AnimationHandler : Singleton<AnimationHandler>, NewSelectionListener
{
    /*
    public List<AnimationClip> animations;
    private Animation anim;
    public GameObject buttonGrid;
    public GameObject button;
    */

    [SerializeField] private AnimationClip starlinkAnim;


    [SerializeField] private AnimationPane animationPane;

    private void Awake()
    {
        SelectionManager.Instance.onLocalSelectionChanged.AddListener(onNewSelection);
    }

    // Start is called before the first frame update
    void Start()
    {
        
        initialise();

        /*
        // instantiate and add Animation component to this object in order to store and play animation clips
        anim = this.AddComponent<Animation>();

        // loop through all clips in the animations list and add them to anim, each clip's name will be based on the incrementing int
        int i = 0;
        foreach(AnimationClip clip in animations)
        {
            string clipName = i.ToString();

            // set clip to work with legacy Animation component
            clip.legacy = true;
            anim.AddClip(clip, clipName);

            // for each animation in the model, add a new button to the hand menu button grid
            GameObject newButton = Instantiate(button, buttonGrid.GetComponent<RectTransform>());

            // make the button play the clip when pressed
            newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => { anim.Play(clipName); });
            i++;
        }
        */
    }

    public void onNewSelection(Transform selection)
    {
        string parentName = SelectionManager.Instance.getModelRoot(SelectionManager.Instance.currentSelection.transform).name;

        if (string.IsNullOrEmpty(parentName))
        {
            DebugConsole.Instance.LogWarning($"couldnt find parent name for component:{selection.name}");
            //set the animation pane's current animations as an empty list
            animationPane.CurrentAnimation = null;
            //populate the animation pane
            animationPane.populateScrollPane();
            return;
        }
        //grab the current selections animation component
        Animation animationComponent = selection.GetComponent<Animation>();
        if (animationComponent == null)
        {
            DebugConsole.Instance.LogWarning($"couldnt find animation for model:{parentName}");
            //set the animation pane's current animations as an empty list
            animationPane.CurrentAnimation = null;
            //populate the animation pane
            animationPane.populateScrollPane();
            return;
        }
        //set the animation panes current animations as animations list
        animationPane.CurrentAnimation = animationComponent;
        //populate the animation pane
        animationPane.populateScrollPane();
    }

    private void initialise()
    {
        /*
        //for each model in all models
        for(int i = 0; i < Config.Instance.AllModels.childCount; i++)
        {
            //get ref of model
            Transform model = Config.Instance.AllModels.GetChild(i);

            //get the animation component of the model or create one
            Animation modelAnimationComponent = model.gameObject.GetComponent<Animation>();
            if (modelAnimationComponent == null)
            {
                modelAnimationComponent = model.gameObject.AddComponent<Animation>();
            }

            //*** this is garbage so we'll have to replace this code later *** <<<<<<<<<<<<<<< OI!
            if (model.name == "Starlink_v1")
            {
                {
                    DebugConsole.Instance.LogDebug($"We have found {model.name}");
                    //add the anim clip to animation component
                    starlinkAnim.legacy = true;
                    modelAnimationComponent.AddClip(starlinkAnim, starlinkAnim.name);
                }
                //get the animations from the model

                //add them to the models dict entry list
            }
        }
        */
    }

    public static List<AnimationClip> getAllAnimationClips(Animation animation)
    {
        List<AnimationClip> animationClips = new List<AnimationClip>();
        foreach(AnimationState state in animation)
        {
            animationClips.Add(state.clip);
        }
        return animationClips;
    }

    
}
