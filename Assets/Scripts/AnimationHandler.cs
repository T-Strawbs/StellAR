using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public List<AnimationClip> animations;
    private Animation anim;
    public GameObject buttonGrid;
    public GameObject button;

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
