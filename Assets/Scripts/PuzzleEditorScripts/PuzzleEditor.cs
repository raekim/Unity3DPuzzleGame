using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleEditor : MonoBehaviour
{
    public GameObject puzzleObject;
    public GameObject CubePrefab;
    public GameObject EdgeCube;

    string puzzleDataName;
    float offSetX;
    float offSetY;
    float offSetZ;

    Puzzle puzzle;

    public Puzzle GetPuzzle()
    {
        return puzzle;
    }

    private void Awake()
    {
        // 3x3x3 퍼즐 초기화
        puzzle = new Puzzle();

        puzzle.answerArray
        = new int[,,]
        {
            {
            {0,0,0 },
            {0,0,0 },
            {0,0,0 }
            },
            {
            {0,0,0 },
            {0,0,0 },
            {0,0,0 }
            },
            {
            {0,0,0 },
            {0,0,0 },
            {0,0,0 }
            },
        };

        ClearPuzzle();

        puzzle.Init();

        offSetX = puzzle.xLen * .5f - .5f;
        offSetY = -puzzle.yLen * .5f + .5f;
        offSetZ = puzzle.zLen * .5f - .5f;

        puzzle.answerArray[1, 1, 1] = 1;

        GeneratePuzzleAnswer(puzzle);
    }

    public void LoadNewPuzzle(Puzzle newPuzzle)
    {
        // 이전 퍼즐 지우기
        ClearPuzzle();

        puzzle.answerArray = newPuzzle.answerArray;

        puzzle.Init();

        // edge 큐브의 사이즈를 퍼즐에 맞춘다
        EdgeCube.transform.localScale = new Vector3(puzzle.xLen, puzzle.yLen, puzzle.zLen);

        offSetX = puzzle.xLen * .5f - .5f;
        offSetY = -puzzle.yLen * .5f + .5f;
        offSetZ = puzzle.zLen * .5f - .5f;

        // 새로운 퍼즐 답안 생성
        GeneratePuzzleAnswer(puzzle);
    }

    void ClearPuzzle()
    {
        // 모든 큐브 오브젝트들 삭제
        foreach (Transform child in puzzleObject.transform)
        {
            Destroy(child.gameObject);
        } 
    }

    Vector3 FromPuzzleIndexToWorldPosition(int[] puzzleIndex)
    {
        int z = puzzleIndex[0];
        int y = puzzleIndex[1];
        int x = puzzleIndex[2];

        Vector3 result = new Vector3(-x, puzzle.yLen - y - 1, -z);
        result += new Vector3(offSetX, offSetY, offSetZ);

        return result;
    }

    Vector3 FromPuzzleIndexToWorldPosition(int z, int y, int x)
    {
        return FromPuzzleIndexToWorldPosition(new int[3] { z, y, x });
    }

    void GeneratePuzzleAnswer(Puzzle puzzle)
    {
        Vector3 cubePosition = Vector3.zero;

        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    // 유니티 좌표계에 맞게 변환
                    if (puzzle.answerArray[z, y, x] == 1)
                    {
                        cubePosition = FromPuzzleIndexToWorldPosition(z, y, x);

                        var cubeObject = Instantiate(CubePrefab, cubePosition, Quaternion.identity, puzzleObject.transform);
                        cubeObject.GetComponent<EditorCube>().cubeIndex = new int[3] { z, y, x };
                    }
                }
            }
        }
    }

    public void AddNewCube(int[] index)
    {
        // 새로운 큐브가 edge 큐브의 범위를 벗어나지 않는지 확인
        bool indexLimitCheck = true;

        for (int i = 0; i < 3; ++i)
        {
            indexLimitCheck &= (0 <= index[i] && index[i] < puzzle.answerArray.GetLength(i));
        }

        if (indexLimitCheck && puzzle.answerArray[index[0], index[1], index[2]] == 0)
        {
            puzzle.answerArray[index[0], index[1], index[2]] = 1;

            Vector3 cubePosition = FromPuzzleIndexToWorldPosition(index);

            var cubeObject = Instantiate(CubePrefab, cubePosition, Quaternion.identity, puzzleObject.transform);
            cubeObject.GetComponent<EditorCube>().cubeIndex = index;
        }
    }
}
