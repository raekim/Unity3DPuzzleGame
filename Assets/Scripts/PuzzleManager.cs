using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public CommandManager commandManager;
    public camRot cameraRot;
    public GameObject buttonsPanel;
    public string PuzzleFileName;   // 플레이 할 퍼즐 이름

    Puzzle puzzle;  // 플레이 할 퍼즐 정보
    Material puzzleBg;

    int currentBreaks = 0;

    private void Awake()
    {
        puzzleBg = Resources.Load<Material>("Materials/bg");

        // 퍼즐 정보 불러오기
        puzzle = SaveLoadManager.LoadPuzzle(PuzzleFileName);

        if(puzzle == null)
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
                    if (puzzle.answerArray[z, y, x] == 1) puzzle.cubes[z, y, x].isAnswerCube = true;
                }
            }
        }
    }

    private void Start()
    {
        
    }

    public Cube[,,] GetPuzzleCubes()
    {
        return puzzle.cubes;
    }

    public int[] GetPuzzleSize()
    {
        int[] res = new int[3];

        res[0] = puzzle.zLen;
        res[1] = puzzle.yLen;
        res[2] = puzzle.xLen;

        return res;
    }

    public void RegisterCubeDestroy()
    {
        currentBreaks++;

        // 퍼즐 완성
        if (currentBreaks == puzzle.breakCount)
        {
            ProcessPuzzleComplete();
        }
    }

    public void ProcessPuzzleComplete()
    {
        commandManager.commandFreeze = true;    // 큐브 색칠이나 보호 금지
        cameraRot.camRotAllowed = false;        // 퍼즐 회전 금지

        StartCoroutine(DetailedPuzzleCompleteEffect());
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

    void HideCommandButtons()
    {
        buttonsPanel.SetActive(false);
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
        HideCommandButtons();

        yield return new WaitForSeconds(.9f);

        // 배경이 점점 어두워짐과 동시에 퍼즐이 빙글빙글 돌아 보기 좋은 각도로 된다
        StartCoroutine(TintBackgroundImage(new Color(.3f, .3f, .3f)));
        yield return StartCoroutine(cameraRot.RotateAroundCompletePuzzle(3f));

        // 완성 된 모습의 퍼즐 공개
        StartCoroutine(TintBackgroundImage(new Color(1f, 1f, 1f)));
        ColorPuzzles();
        //PuzzleWhiteFlashEffect();


        // -----> 구현중
    }

    private void OnApplicationQuit()
    {
        puzzleBg.color = Color.white;
    }
}
