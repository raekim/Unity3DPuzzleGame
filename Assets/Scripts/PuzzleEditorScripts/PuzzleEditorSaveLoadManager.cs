using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleEditorSaveLoadManager : MonoBehaviour
{
    public Text loadFileNameText;
    public Text saveFileNameText;
    public PuzzleEditor puzzleEditor;

    public void SavePuzzleFile()
    {
        Puzzle puzzleToSave = puzzleEditor.GetPuzzle();
        puzzleToSave.Init();
        puzzleToSave.puzzleName = saveFileNameText.text;

        // 에디터에서 만든 퍼즐은 답안의 형태만을 가지고 있다.
        // 이것을 PuzzleSolver로 풀어서 number clue를 떼어네는 등의 사후처리를 한 후에
        // 최종적인 퍼즐 데이터로써 저장한다.
        // 이렇게 저장된 퍼즐 데이터 파일은 플레이 씬에서 그대로 불러와 게임 플레이가 가능하다.

        //초기 number clue 생성
        PuzzleSolver solver = new PuzzleSolver();
        solver.ApplyInitialNumberClues(puzzleToSave);

        // number clue 떼어내는 작업
        solver.ProcessPuzzle(puzzleToSave);

        SaveLoadManager.SavePuzzle(puzzleToSave, saveFileNameText.text);
    }

    public void LoadPuzzleFile()
    {
        Puzzle newPuzzle = SaveLoadManager.LoadPuzzle(loadFileNameText.text);

        if(newPuzzle == null)
        {
            Debug.Log("퍼즐 로드 실패");
            return;
        }

        puzzleEditor.LoadNewPuzzle(newPuzzle);
    }
}
