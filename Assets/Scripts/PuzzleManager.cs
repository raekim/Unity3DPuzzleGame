using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    public GameObject slicersParent;
    public GameObject edgeCube;
    public CommandManager commandManager;
    public CameraRotationManager cameraRot;
    public Text puzzleFileName; // 퍼즐 파일 열기에 쓰이는 이름
    public Text currentOpenPuzzleNameDisplay;
    public Text completeTextDisplay;

    Slicer[] slicers;

    Puzzle puzzle;  // 플레이 할 퍼즐 정보
    Material puzzleBg;

    int currentBreaks = 0;  // 현재까지 부순 큐브의 수. 퍼즐 complete 판정에 쓰인다

    private void Start()
    {
        LoadAndGeneratePuzzle("Sofa");
        puzzleBg = Resources.Load<Material>("Materials/bg");

        edgeCube.SetActive(false);
    }

    public void SetEdgeCubeVisibility(bool value)
    {
        edgeCube.SetActive(value);
    }

    public void SetSlicers(Slicer[] _slicers)
    {
        slicers = _slicers;
    }

    public void LoadAndGeneratePuzzle()
    {
        LoadAndGeneratePuzzle(puzzleFileName.text);
    }

    public void LoadAndGeneratePuzzle(string puzzleFileName)
    {
        currentBreaks = 0;

        commandManager.commandFreeze = false;
        cameraRot.camRotAllowed = true;

        completeTextDisplay.gameObject.SetActive(false);

        // 퍼즐 정보 불러오기
        puzzle = SaveLoadManager.LoadPuzzle(puzzleFileName);

        currentOpenPuzzleNameDisplay.text = "퍼즐 이름 : " + puzzleFileName;

        if (puzzle == null)
        {
            Debug.Log("Error : 퍼즐 불러오기 실패!");
            Application.Quit();
        }

        // Cube들로 이루어진 퍼즐 생성
        PuzzleGenerator puzzleGenerator = GetComponent<PuzzleGenerator>();
        puzzleGenerator.GeneratePuzzle(puzzle);

        // 퍼즐의 큐브들 중 정답 큐브 mark
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    puzzle.cubes[z, y, x].isAnswerCube = false;
                    if (puzzle.answerArray[z, y, x] == 1) puzzle.cubes[z, y, x].isAnswerCube = true;
                }
            }
        }

        for (int i = 0; i < slicers.Length; ++i)
        {
            slicers[i].InitSlicer(puzzle);
        }

        // Edge Cube 사이즈를 새로운 퍼즐에 맞게 조절
        edgeCube.transform.localScale = new Vector3(puzzle.xLen + 0.05f, puzzle.yLen + 0.05f, puzzle.zLen + 0.05f);
    }

    public void RegisterCubeDestroy()
    {
        currentBreaks++;
        Debug.Log(currentBreaks + "/" + puzzle.breakCount);

        // 퍼즐 완성
        if (currentBreaks == puzzle.breakCount)
        {
            ProcessPuzzleComplete();
            completeTextDisplay.gameObject.SetActive(true);
        }
    }

    public void ProcessPuzzleComplete()
    {
        commandManager.commandFreeze = true;    // 큐브 색칠이나 보호 금지
        cameraRot.camRotAllowed = true;        // 퍼즐 회전 금지

        //  StartCoroutine(DetailedPuzzleCompleteEffect());
    }

    void HideNumberCluesOnCubes()
    {
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    if (puzzle.answerColorIndex[z, y, x] != -1)
                    {
                        puzzle.cubes[z, y, x].HideNumberClue();
                    }
                }
            }
        }
    }

    IEnumerator TintBackgroundImage(Color targetColor)
    {
        // 배경 이미지를 서서히 색상변경한다
        float cnt = 0f;
        Color initColor = puzzleBg.color;

        while (cnt < 3.5f)
        {
            puzzleBg.color = Color.Lerp(initColor, targetColor, cnt);

            cnt += Time.deltaTime;
            yield return null;
        }
    }

    // 정답 색상으로 색칠
    void ColorPuzzles()
    {
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    int answerColorIndex = puzzle.answerColorIndex[z, y, x];
                    if (answerColorIndex != -1)
                    {
                        puzzle.cubes[z, y, x].ClearFaces();
                        puzzle.cubes[z, y, x].SetColor(puzzle.answerColors[answerColorIndex], (int)Cube.MATERIAL_INDEX.BACKGROUND);
                    }
                }
            }
        }
    }

    IEnumerator DetailedPuzzleCompleteEffect()
    {
        HideNumberCluesOnCubes();

        yield return new WaitForSeconds(.9f);

        // 배경이 점점 어두워짐과 동시에 퍼즐이 빙글빙글 돌아 보기 좋은 각도로 된다
        StartCoroutine(TintBackgroundImage(new Color(.3f, .3f, .3f)));
        //yield return StartCoroutine(cameraRot.RotateAroundCompletePuzzle(3f));

        // 완성 된 모습의 퍼즐 공개
        StartCoroutine(TintBackgroundImage(new Color(1f, 1f, 1f)));
        ColorPuzzles();
        //PuzzleWhiteFlashEffect();


        // -----> 구현중
    }
}
