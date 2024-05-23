using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;

/// <summary>
/// enum for the explosion status of an explodable.
/// </summary>
public enum ExplosionStatus
{
    /// <summary>
    /// EXPLODED: The explodable has been exploded.
    /// </summary>
    EXPLODED,
    /// <summary>
    /// EXPLODABLE: The explodable is able to be exploded.
    /// </summary>
    EXPLODABLE,
    /// <summary>
    /// INACTIVE: The explodable cannot be exploded.
    /// </summary>
    INACTIVE
}

public class Explodable : MonoBehaviour
{
    [SerializeField] private List<Transform> children;

    [SerializeField] private Explodable parent = null;

    [SerializeField] private const float EXPLOSION_SPEED = 2f;
    [SerializeField] private const float COLLAPSE_SPEED = 2f;
    [SerializeField] private const float STOP_DISTANCE = 1.5f;
    [SerializeField] private const float CLAMPING_DISTANCE = 1000f;

    [SerializeField] private SelectableManipulator selectableManipulator;

    [SerializeField] private ExplosionStatus explosionStatus;
    
    [SerializeField] private Vector3 preexplosionPosition;
    [SerializeField] private Quaternion preexplosionRotation;
    [SerializeField] private Vector3 preexplosionScale;

    public void Start()
    {
        //initialise parent explodable
        setParent();
        //initialise Explodability 
        initialiseExplodability();
        //set OG transform
        preexplosionPosition = transform.localPosition;
        preexplosionRotation = transform.localRotation;
        preexplosionScale = transform.localScale;
    }
    /// <summary>
    /// sets the parent of this explodable
    /// </summary>
    private void setParent()
    {
        if(!transform.parent)
        {
            parent = null;
            //Debug.Log($"The parent of \"{transform.name}\" doesnt exist.");
            return;
        }
        parent = transform.parent.GetComponent<Explodable>();
        /*
        if(!parent)
            Debug.Log($"The parent of \"{transform.name}\" is not an explodable.");
        */
    }

    public Explodable getParent()
    {
        return parent;
    }

    /// <summary>
    /// initialises the explodable
    /// </summary>
    private void initialiseExplodability()
    {
        //get children
        children = new List<Transform>();
        for(int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
        //ensure that this explodable can be exploded if it is the root
        if (!parent)
        {
            explosionStatus = ExplosionStatus.EXPLODABLE;
        }
        else
        {
            explosionStatus = ExplosionStatus.INACTIVE;
        }
    }
    /// <summary>
    /// sets this explodable's selectableManipulator
    /// </summary>
    /// <param name="selectableManipulator">the selectable manipulator</param>
    public void setSelectableManipulator(SelectableManipulator selectableManipulator)
    {
        this.selectableManipulator = selectableManipulator;
    }
    /// <summary>
    /// Explodes this GOs children
    /// </summary>
    public void explode()
    {
        //if this explodable is unable to explode
        if (explosionStatus == ExplosionStatus.EXPLODED || explosionStatus == ExplosionStatus.INACTIVE)
        {
            DebugConsole.Instance.LogWarning($"attempted to explode {transform.name} but it isnt explodable.");
            return;
        }
        //we're exploding
        explosionStatus = ExplosionStatus.EXPLODED;

        //check if this object is MT
        if(isEmptyObject())
        {
            selectableManipulator.enabled = false;
        }
        //for each child
        foreach(Transform child in children)
        {
            //grab its explodable component
            Explodable childExplodable = child.GetComponent<Explodable>();
            if(!childExplodable)
            {
                Debug.Log($"child '{child.name}' is not an explodable.");
                return;
            }
            //activate child selectable manipulator
            SelectableManipulator childSelectableManipulator = child.GetComponent<SelectableManipulator>();
                if (childSelectableManipulator)
                    childSelectableManipulator.enabled = true;
            //if child has children
            if (child.childCount > 0)
            {
                //set childs explosion status to explodable as it is now explodable
                childExplodable.explosionStatus = ExplosionStatus.EXPLODABLE;
            }
            //get the angle between the parent and child
            Vector3 moveTrajectory = calculateTrajectory(childExplodable);
            //move the child in that direction
            childExplodable.onExplode(moveTrajectory);
        }

    }
    /// <summary>
    /// Calculates the trajectory between the parent and the childs global positions
    /// </summary>
    /// <param name="child">
    /// The child Explodable.
    /// </param>
    /// <returns>
    /// Vector3: The trajectory of the child.
    /// </returns>
    private Vector3 calculateTrajectory(Explodable child)
    {
        //get the location of the childs bounding box if it has one
        MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
        //get the parents too
        MeshRenderer parentRenderer = GetComponent<MeshRenderer>();

        //get the default positions if we dont have renderers
        Vector3 childCentre = child.transform.position;
        Vector3 parentCentre = transform.position;
        
        //check if the child has a renderer
        if (childRenderer)
        {
            Bounds childBounds = childRenderer.bounds;
            childCentre = childBounds.center;
        }
        //check if the parent has a renderer
        if (parentRenderer)
        {
            Bounds parentBounds = parentRenderer.bounds;
            parentCentre = parentBounds.center;
        }
        //calculate trajectory
        Vector3 trajectory = childCentre - parentCentre;

        return trajectory;
    }
    /// <summary>
    /// callback for invoking the move coroutine of the child explodable.
    /// </summary>
    /// <param name="moveTrajectory">
    /// The trajectory of the child.
    /// </param>
    private void onExplode(Vector3 moveTrajectory)
    {
        //move this explodable
        StartCoroutine(translateExplosion(moveTrajectory));
    }
    /// <summary>
    /// moves the explodable from their current local position outwards in the movementDirection.
    /// </summary>
    /// <param name="moveDirection">
    /// The tragectory of the movement
    /// </param>
    private IEnumerator translateExplosion(Vector3 moveDirection)
    {
        //prevent the manipulation of this explodable and it is children while moving
        switchManipuation(this, false);
        //get the intial position from where we're moving from
        Vector3 intialPos = transform.localPosition;
        //set the target pos;
        Vector3 targetpos = intialPos + transform.InverseTransformDirection(moveDirection).normalized * STOP_DISTANCE;
        //set the time and duration of the loop and lerp
        float time = 0f;
        float duration = STOP_DISTANCE / EXPLOSION_SPEED;
        //while we havent reached our destiniation
        while (time < duration)
        {
            //move us
            transform.localPosition = Vector3.Lerp(intialPos, targetpos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        //give back our manipulation
        switchManipuation(this,true);
    }
    /// <summary>
    /// Recursive method for switching the manipulation of explodables on or off.
    /// </summary>
    /// <param name="currentExplodable">the current explodable</param>
    /// <param name="allowAll">bool for switching the manipulation on or off</param>
    private void switchManipuation(Explodable currentExplodable, bool allowAll)
    {
        //get the current explodable's selectable manipulator
        SelectableManipulator selectableManipulator = currentExplodable.GetComponent<SelectableManipulator>();
        if(selectableManipulator)
        {
            if (allowAll)
                //allow all manipulations
                selectableManipulator.AllowedManipulations =
                    MixedReality.Toolkit.TransformFlags.Move |
                    MixedReality.Toolkit.TransformFlags.Rotate |
                    MixedReality.Toolkit.TransformFlags.Scale;
            else
                //revoke all manipulations
                selectableManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.None;

        }
        //for each child of current explodable
        for(int i = 0; i < currentExplodable.transform.childCount; i ++)
        {
            //grab it as an explodable
            Explodable child = currentExplodable.transform.GetChild(i).GetComponent<Explodable>();
            if (!child)
                continue;
            //recursively switch the decendant's manip setting
            switchManipuation(child, allowAll);
        }
    }
    /// <summary>
    /// collapses all members of this explodables tree.
    /// </summary>
    public void collapseAll()
    {
        Debug.Log($"collapsing all from {transform.name}");
        //find root explodable
        Explodable currentExplodable = this;
        while(currentExplodable.parent != null)
        {
            currentExplodable = currentExplodable.parent;
        }
        //collapse children
        collapseChildren(currentExplodable);
        //set current explosives explosive status to explodable
        currentExplodable.explosionStatus = ExplosionStatus.EXPLODABLE;
        //check if the current explodable is an empty object
        if(currentExplodable.isEmptyObject())
        {
            //enable its manipulator
            currentExplodable.selectableManipulator.enabled = true;
        }
    }
    /// <summary>
    /// Collapses this explodables branch 
    /// </summary>
    public void collapse()
    {
        Debug.Log($"collapsing{transform.name}");
        //get pred that is marked explodable
        Explodable pred = findExplodablePredecessor(this);
        //get preds parent
        Explodable predParent = pred.parent;
        if (!predParent)
            return;
        //recursively collapse children
        collapseChildren(predParent);
        //set parents explosion status to explodable
        parent.explosionStatus = ExplosionStatus.EXPLODABLE;
        //check if the current explodable is an empty object
        if (predParent.isEmptyObject())
        {
            //enable its manipulator
            predParent.selectableManipulator.enabled = true;
        }
    }
    /// <summary>
    /// Recursive method for collapsing all children of a parent.
    /// </summary>
    /// <param name="parent">The parent</param>
    /// <param name="hostTransform">the root parent's transform to set the host transfrom of the children</param>
    private void collapseChildren(Explodable parentExplodable)
    {
        //for each child of parent
        for(int i = 0; i < parentExplodable.transform.childCount; i++)
        {
            //get the child
            Explodable child = parentExplodable.transform.GetChild(i).GetComponent<Explodable>();
            if (!child)
                continue;
            //check if child has exploded
            if(child.explosionStatus == ExplosionStatus.EXPLODED)
                //collapse the child's children
                collapseChildren(child);
            //set the child's explosion status to Innactive
            child.explosionStatus = ExplosionStatus.INACTIVE;
            //disable the childs selectable manipulator 
            SelectableManipulator childSelectableManipulator = child.GetComponent<SelectableManipulator>();
            if (childSelectableManipulator)
                childSelectableManipulator.enabled = false;
            //move child back to parent
            child.onCollapse();
        }
    }
    /// <summary>
    /// callback to initiate the collapse translation of explodables
    /// </summary>
    private void onCollapse()
    {
        StartCoroutine(translateCollapse());
    }
    /// <summary>
    /// Moves the explodable back to its pre explosion local position.
    /// </summary>
    /// <returns></returns>
    private IEnumerator translateCollapse()
    {
        //deactive manipulation
        switchManipuation(this, false);
        //set the starting position of the movement
        Vector3 initalPosition = transform.localPosition;
        //set the starting rotation
        Quaternion initialRotation = transform.localRotation;
        //set the starting scale
        Vector3 initalScale = transform.localScale;
        float time = 0f;
        float duration = 1f / COLLAPSE_SPEED;
        //while we arent at the preexplostion position
        while(time < duration)
        {
            //move the component back to its preeplosion position
            transform.localPosition = Vector3.Lerp(initalPosition, preexplosionPosition, time / duration);
            //rotate back into the pre explosion rotation
            transform.localRotation = Quaternion.Lerp(initialRotation, preexplosionRotation, time / duration);
            //scale back into the preexplosion scale
            transform.localScale = Vector3.Lerp(initalScale, preexplosionScale, time / duration);
            //clamp the postion to valid bounds so models are less likely to freak out
            transform.localPosition = Vector3.ClampMagnitude(transform.localPosition, CLAMPING_DISTANCE);
            time += Time.deltaTime;
            yield return null;
        }
        //snap us to the exact pre-explosion position
        transform.localPosition = Vector3.ClampMagnitude(preexplosionPosition, CLAMPING_DISTANCE);
        //same with the rotation
        transform.localRotation = preexplosionRotation;
        //and scale 
        transform.localScale = preexplosionScale;
        //reactivate manipulation
        switchManipuation(this, true);
    }
    /// <summary>
    /// finds the highest Explodable in this explodables tree.
    /// </summary>
    /// <returns>
    /// Explodable: the predecessor that is Explodable in the tree.
    /// </returns>
    public Explodable findExplodablePredecessor()
    {
        return findExplodablePredecessor(this);
    }
    /// <summary>
    /// recurseive method for finding the highest explodable in the tree
    /// </summary>
    /// <param name="currentExplodable">
    /// The current explodable we're processing over.
    /// </param>
    /// <returns>
    /// Explodable: the highest Explodable in the tree.
    /// </returns>
    private Explodable findExplodablePredecessor(Explodable currentExplodable)
    { 
        //check if the parent of the current explodable is null
        if (currentExplodable.parent == null)
        {
            return currentExplodable;
        }
        //check if the current explodable is a leaf
        if(currentExplodable.parent.explosionStatus == ExplosionStatus.EXPLODED)
        {
            return currentExplodable;
        }
        //check if the parent is explodable
        if (currentExplodable.explosionStatus != ExplosionStatus.EXPLODABLE)
        {
            //its not so we climb up to the next parent
            return findExplodablePredecessor(currentExplodable.parent);
        }
        //the parent is explodable so return parent
        return currentExplodable.parent;
    }

    private bool isEmptyObject()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if(!meshRenderer && !skinnedMeshRenderer)
            return true;
        return false;
    }

}
 