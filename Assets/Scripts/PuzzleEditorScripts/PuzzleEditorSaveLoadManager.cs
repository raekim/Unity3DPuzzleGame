using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleEditorSaveLoadManager : MonoBehaviour
{
    Text fileNameText;
    PuzzleEditor puzzleEditor;

    void SavePuzzleFile()
    {

    }

    void LoadPuzzleFile()
    {
        puzzleEditor.ClearPuzzle();
    }
}
