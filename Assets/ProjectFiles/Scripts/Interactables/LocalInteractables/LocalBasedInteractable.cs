using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Interactable Concretion for Interactables whos behaviour is handled offline.
/// </summary>
public class LocalBasedInteractable : Interactable
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
