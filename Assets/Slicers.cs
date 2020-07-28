using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicers : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    Slicer[] childSlicers;

    private void Awake()
    {
        childSlicers = GetComponentsInChildren<Slicer>();
        puzzleManager.SetSlicers(childSlicers);
    }
}
