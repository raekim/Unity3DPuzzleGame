//#define EFFECTS
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenerator : MonoBehaviour
{
    public GameObject CubePrefab;
    public GameObject puzzleObject;

    public void GeneratePuzzle(Puzzle puzzle)
    {
        puzzle.cubes = new Cube[puzzle.zLen, puzzle.yLen, puzzle.xLen];

        // 답안을 감싸는 큐브들 생성
        GeneratePuzzleCube(puzzle);

        // number clue 붙이기
        ApplyNumberClues(puzzle);
    }

    void GeneratePuzzleCube(Puzzle puzzle)
    {
        Transform puzzleParent = puzzleObject.transform;

        puzzleParent.gameObject.GetComponent<BoxCollider>().size = new Vector3(puzzle.zLen, puzzle.yLen, puzzle.xLen);

        Vector3 cubePosition = Vector3.zero;

        float offSetX = puzzle.xLen * .5f - .5f;
        float offSetY = -puzzle.yLen * .5f + .5f;
        float offSetZ = puzzle.zLen * .5f - .5f;

        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    // 유니티 좌표계에 맞게 변환
                    cubePosition = new Vector3(-x, puzzle.yLen - y - 1, -z);
                    cubePosition += new Vector3(offSetX, offSetY, offSetZ);

                    var cubeObject = Instantiate(CubePrefab, cubePosition, Quaternion.identity, puzzleParent);
                    puzzle.cubes[z, y, x] = cubeObject.GetComponent<Cube>();
                }
            }
        }
    }

    void ApplyNumberClues(Puzzle puzzle)
    {
        for (int z = 0; z < puzzle.zLen; ++z)
        {
            for (int y = 0; y < puzzle.yLen; ++y)
            {
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    puzzle.cubes[z, y, x].SetFrontBackNumberClue(puzzle.numberClue[0][y, x].number, puzzle.numberClue[0][y, x].shape);
                    puzzle.cubes[z, y, x].SetTopBottomNumberClue(puzzle.numberClue[1][x, z].number, puzzle.numberClue[1][x, z].shape);
                    puzzle.cubes[z, y, x].SetRightLeftNumberClue(puzzle.numberClue[2][y, z].number, puzzle.numberClue[2][y, z].shape);
                }
            }
        }
    }
}
