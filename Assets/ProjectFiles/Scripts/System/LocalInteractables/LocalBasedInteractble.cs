using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalBasedInteractble : Interactable
{
    public override void explodeInteractable()
    {
        explodable.explode();
    }

    public override void collapseInteractable(bool isSingleCollapse)
    {
        if(isSingleCollapse)
            explodable.collapseSingle();
        else
            explodable.collapseAll();
    }
}
