using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzlePlayLoader : MonoBehaviour
{
    public Text loadFileNameText;
    
    public void PuzzleLoad()
    {
        Puzzle newPuzzle = SaveLoadManager.LoadPuzzle(loadFileNameText.text);

        if (newPuzzle == null)
        {
            Debug.Log("퍼즐 로드 실패");
            return;
        }
    }
}
