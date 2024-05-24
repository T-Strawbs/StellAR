using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface SelectionSubcriber
{
    public void subscribe();

    public void updateSelection(Transform selection);
}
