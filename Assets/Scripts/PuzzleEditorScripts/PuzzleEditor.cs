using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void ToPlayScene()
    {
        SceneManager.LoadScene(0);
    }

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
            {1,1,1 },
            {1,1,1 },
            {1,1,1 }
            },
            {
            {1,1,1 },
            {1,1,1 },
            {1,1,1 }
            },
            {
            {1,1,1 },
            {1,1,1 },
            {1,1,1 }
            },
        };

        puzzle.answerArray[1, 1, 1] = 1;

        InitAndGeneratePuzzle(); 
    }

    public void LoadNewPuzzle(Puzzle newPuzzle)
    {
        puzzle.answerArray = newPuzzle.answerArray;
        InitAndGeneratePuzzle();
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

    void InitAndGeneratePuzzle()
    {
        puzzle.Init();

        offSetX = puzzle.xLen * .5f - .5f;
        offSetY = -puzzle.yLen * .5f + .5f;
        offSetZ = puzzle.zLen * .5f - .5f;

        ClearPuzzle();
        GeneratePuzzleAnswer(puzzle);

        // edge 큐브의 사이즈를 퍼즐에 맞춘다
        EdgeCube.transform.localScale = new Vector3(puzzle.xLen, puzzle.yLen, puzzle.zLen);
    }

    void ShrinkPuzzleZAxis(int[] destroyCubeIndex, int shrinkNum)
    {
        int[,,] newAnswerArray = new int[puzzle.zLen - shrinkNum, puzzle.yLen, puzzle.xLen];

        int zDiff = (destroyCubeIndex[0] == 0 ? shrinkNum : 0);

        // z축 방향으로 answerArray 줄여서 복사
        for (int z = 0; z < puzzle.zLen - shrinkNum; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    newAnswerArray[z, y, x] = puzzle.answerArray[z + zDiff, y, x];
                }
            }
        }

        puzzle.answerArray = newAnswerArray;

        InitAndGeneratePuzzle();
    }
                                                 
    void ShrinkPuzzleYAxis(int[] destroyCubeIndex, int shrinkNum)
    {
        int[,,] newAnswerArray = new int[puzzle.zLen, puzzle.yLen - shrinkNum, puzzle.xLen];

        int yDiff = (destroyCubeIndex[1] == 0 ? shrinkNum : 0);

        // y축 방향으로 answerArray 줄여서 복사
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen - shrinkNum; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    newAnswerArray[z, y, x] = puzzle.answerArray[z, y + yDiff, x];
                }
            }
        }

        puzzle.answerArray = newAnswerArray;

        InitAndGeneratePuzzle();
    }
                                                 
    void ShrinkPuzzleXAxis(int[] destroyCubeIndex, int shrinkNum)
    {
        int[,,] newAnswerArray = new int[puzzle.zLen, puzzle.yLen, puzzle.xLen - shrinkNum];

        int xDiff = (destroyCubeIndex[2] == 0 ? shrinkNum : 0);

        // x축 방향으로 answerArray 줄여서 복사
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen - shrinkNum; ++x)
                {
                    newAnswerArray[z, y, x] = puzzle.answerArray[z, y, x + xDiff];
                }
            }
        }

        puzzle.answerArray = newAnswerArray;

        InitAndGeneratePuzzle();
    }

    void EnlargePuzzleZAxis(int[] newCubeIndex)
    {
        int[,,] newAnswerArray = new int[puzzle.zLen + 1, puzzle.yLen, puzzle.xLen];

        int zDiff = (newCubeIndex[0] < 0 ? 1 : 0);

        // z축 방향으로 answerArray 늘려서 복사
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            if (z + zDiff < 0) continue;
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    newAnswerArray[z + zDiff, y, x] = puzzle.answerArray[z, y, x];
                }
            }
        }

        puzzle.answerArray = newAnswerArray;

        InitAndGeneratePuzzle();
    }

    void EnlargePuzzleYAxis(int[] newCubeIndex)
    {
        int[,,] newAnswerArray = new int[puzzle.zLen, puzzle.yLen + 1, puzzle.xLen];

        int yDiff = (newCubeIndex[1] < 0 ? 1 : 0);

        // y축 방향으로 answerArray 늘려서 복사
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    newAnswerArray[z, y + yDiff, x] = puzzle.answerArray[z, y, x];
                }
            }
        }

        puzzle.answerArray = newAnswerArray;

        InitAndGeneratePuzzle();
    }

    void EnlargePuzzleXAxis(int[] newCubeIndex)
    {
        int[,,] newAnswerArray = new int[puzzle.zLen, puzzle.yLen, puzzle.xLen + 1];

        int xDiff = (newCubeIndex[2] < 0 ? 1 : 0);

        // x축 방향으로 answerArray 늘려서 복사
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    newAnswerArray[z, y, x + xDiff] = puzzle.answerArray[z, y, x];
                }
            }
        }

        puzzle.answerArray = newAnswerArray;

        InitAndGeneratePuzzle();
    }

    public void DestoryCube(int[] index)
    {
        // 마지막 남은 큐브는 부수지 않는다
        if ((puzzle.zLen * puzzle.yLen * puzzle.xLen) - puzzle.breakCount == 1) return;

        puzzle.answerArray[index[0], index[1], index[2]] = 0;

        // front 또는 back 면의 마지막 큐브를 부순 경우
        if (index[0] == 0 || index[0] == puzzle.zLen - 1)
        {
            bool faceBlank = true;
            int shrinkNum = 0;
            int indexDiff = 0;

            while (faceBlank && 0 <= index[0] + indexDiff && index[0] + indexDiff < puzzle.zLen)
            {
                for (int y = 0; y < puzzle.yLen; ++y)
                {
                    if (!faceBlank) break;
                    for (int x = 0; x < puzzle.xLen; ++x)
                    {
                        if (puzzle.answerArray[index[0] + indexDiff, y, x] == 1)
                        {
                            faceBlank = false;
                            break;
                        }
                    }
                }

                if (faceBlank)
                {
                    shrinkNum++;
                    indexDiff += (index[0] == 0 ? 1 : -1);
                }
            }

            if (shrinkNum > 0 && puzzle.zLen > shrinkNum)
            {
                ShrinkPuzzleZAxis(index, shrinkNum);

            }
        }

        // top 또는 bottom 면의 마지막 큐브를 부순 경우
        if (index[1] == 0 || index[1] == puzzle.yLen - 1)
        {
            bool faceBlank = true;
            int shrinkNum = 0;
            int indexDiff = 0;

            while (faceBlank && 0 <= index[1] + indexDiff && index[1] + indexDiff < puzzle.yLen)
            {
                for (int z = 0; z < puzzle.zLen; ++z)
                {
                    if (!faceBlank) break;
                    for (int x = 0; x < puzzle.xLen; ++x)
                    {
                        if (puzzle.answerArray[z, index[1] + indexDiff, x] == 1)
                        {
                            faceBlank = false;
                            break;
                        }
                    }
                }

                if (faceBlank)
                {
                    shrinkNum++;
                    indexDiff += (index[1] == 0 ? 1 : -1);
                }
            }

            if (shrinkNum > 0 && puzzle.yLen > shrinkNum)
            {
                ShrinkPuzzleYAxis(index, shrinkNum);
            }
        }

        // left 또는 right 면의 마지막 큐브를 부순 경우
        if (index[2] == 0 || index[2] == puzzle.xLen - 1)
        {
            bool faceBlank = true;
            int shrinkNum = 0;
            int indexDiff = 0;

            while (faceBlank && 0 <= index[2] + indexDiff && index[2] + indexDiff < puzzle.xLen)
            {
                for (int z = 0; z < puzzle.zLen; ++z)
                {
                    if (!faceBlank) break;
                    for (int y = 0; y < puzzle.yLen; ++y)
                    {
                        if (puzzle.answerArray[z, y, index[2] + indexDiff] == 1)
                        {
                            faceBlank = false;
                            break;
                        }
                    }
                }

                if (faceBlank)
                {
                    shrinkNum++;
                    indexDiff += (index[2] == 0 ? 1 : -1);
                }
            }

            if (shrinkNum > 0 && puzzle.xLen > shrinkNum)
            {
                ShrinkPuzzleXAxis(index, shrinkNum);
            }
        }

        InitAndGeneratePuzzle();
    }

    public void AddNewCube(int[] index)
    {
        bool makeNewCube = true;

        // 새로운 큐브가 edge 큐브의 범위를 벗어나지 않는지 확인
        for (int i = 0; i < 3; ++i)
        {
            if (index[i] < 0 || index[i] >= puzzle.answerArray.GetLength(i))
            {
                // 새로운 큐브가 edge 큐브의 범위를 벗어남.
                // edge 큐브를 크게 키우던가, edge 큐브의 한 축의 길이가 9를 넘게 생겼으면 큐브 생성을 취소
                switch(i)
                {
                    case 0: // z
                        if(puzzle.zLen < 9)
                        {
                            EnlargePuzzleZAxis(index);
                            index[0] = (index[0] < 0 ? 0 : index[0]);
                        }
                        else
                        {
                            makeNewCube = false;
                        }
                        break;
                    case 1: // y
                        if (puzzle.yLen < 9)
                        {
                            EnlargePuzzleYAxis(index);
                            index[1] = (index[1] < 0 ? 0 : index[1]);
                        }
                        else
                        {
                            makeNewCube = false;
                        }
                        break;
                    case 2: // x
                        if (puzzle.xLen < 9)
                        {
                            EnlargePuzzleXAxis(index);
                            index[2] = (index[2] < 0 ? 0 : index[2]);
                        }
                        else
                        {
                            makeNewCube = false;
                        }
                        break;
                }
            }
        }

        // index 자리에 세로운 큐브 생성
        if (makeNewCube && puzzle.answerArray[index[0], index[1], index[2]] == 0)
        {
            puzzle.answerArray[index[0], index[1], index[2]] = 1;

            Vector3 cubePosition = FromPuzzleIndexToWorldPosition(index);

            var cubeObject = Instantiate(CubePrefab, cubePosition, Quaternion.identity, puzzleObject.transform);
            cubeObject.GetComponent<EditorCube>().cubeIndex = index;
        }
    }
}
