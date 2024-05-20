using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExplostionTest : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            testExplosion();
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            testCollapseAll();
        if (Input.GetKeyDown(KeyCode.C))
            testCollapse();
    }

    public void testExplosion()
    {
        if(SelectionManager.currentSelection == null)
        {
            Debug.Log($"current selection is null");
            return;
        }
        Explodable explodable = SelectionManager.currentSelection;
        if(!explodable)
        {
            Debug.Log($"current selection {SelectionManager.currentSelection.name} is not an Explodable");
            return;
        }
        explodable.explode();
    }

    public void testCollapse()
    {
        if (SelectionManager.currentSelection == null)
        {
            Debug.Log($"current selection is null");
            return;
        }
        Explodable explodable = SelectionManager.currentSelection;
        if (!explodable)
        {
            Debug.Log($"current selection {SelectionManager.currentSelection.name} is not an Explodable");
            return;
        }
        explodable.collapse();
    }

    public void testCollapseAll()
    {
        if (SelectionManager.currentSelection == null)
        {
            Debug.Log($"current selection is null");
            return;
        }
        Explodable explodable = SelectionManager.currentSelection;
        if (!explodable)
        {
            Debug.Log($"current selection {SelectionManager.currentSelection.name} is not an Explodable");
            return;
        }
        explodable.collapseAll();
    }
}
