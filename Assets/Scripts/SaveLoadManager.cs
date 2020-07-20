using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
    public static void SavePuzzle(Puzzle puzzle)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.dataPath + @"\Resources\Testpuzzle.txt", FileMode.Create);

        PuzzleData data = new PuzzleData(puzzle);

        bf.Serialize(stream, data);
        stream.Close();
    }

    public static Puzzle LoadPuzzle()
    {
        if(File.Exists(Application.dataPath + @"\Resources\Testpuzzle.txt"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.dataPath + @"\Resources\Testpuzzle.txt", FileMode.Open);

            PuzzleData data = bf.Deserialize(stream) as PuzzleData;

            stream.Close();

            // puzzleData를 puzzle로 만들기
            Puzzle puzzle = new Puzzle();

            puzzle.puzzleName = data.puzzleName;
            puzzle.answerArray = data.answerArray;
            puzzle.answerColorIndex = data.answerColorIndex;

            puzzle.zLen = data.zLen;
            puzzle.yLen = data.yLen;
            puzzle.xLen = data.xLen;
            puzzle.breakCount = data.breakCount;

            puzzle.numberClue = new List<Puzzle.NumberClue[,]>();
            puzzle.numberClue.Add(new Puzzle.NumberClue[puzzle.yLen, puzzle.xLen]); // front-back;
            puzzle.numberClue.Add(new Puzzle.NumberClue[puzzle.xLen, puzzle.zLen]); // top-bottom;
            puzzle.numberClue.Add(new Puzzle.NumberClue[puzzle.yLen, puzzle.zLen]); // right-left;

            var dataNumberClue = data.numberClue;

            for (int face = 0; face < 3; ++face)
            {
                for (int i = 0; i < dataNumberClue[face].GetLength(0); ++i)
                {
                    for (int j = 0; j < dataNumberClue[face].GetLength(1); ++j)
                    {
                        if (dataNumberClue[face][i, j] == -1)
                        {
                            puzzle.numberClue[face][i, j].number = -1;
                        }
                        else
                        {
                            puzzle.numberClue[face][i, j] = new Puzzle.NumberClue();
                            puzzle.numberClue[face][i, j].number = dataNumberClue[face][i, j] / 10;
                            puzzle.numberClue[face][i, j].shape = (Puzzle.CLUE_SHAPE)(dataNumberClue[face][i, j] % 10);
                        }
                    }
                }
            }

            return puzzle;
        }
        return null;
    }
}

[Serializable]
public class PuzzleData
{
    public string puzzleName;
    public int[,,] answerArray;
    public int[,,] answerColorIndex;
    public List<int[,]> numberClue; // List인덱스 : face, 각 원소 : (숫자)*10 + (모양). 숫자가 -1이면 없는 number clue
    public int zLen;
    public int yLen;
    public int xLen;
    public int breakCount;

    public PuzzleData(Puzzle puzzle)
    {
        puzzleName = puzzle.puzzleName;
        answerArray = puzzle.answerArray;
        answerColorIndex = puzzle.answerColorIndex;

        zLen = puzzle.zLen;
        yLen = puzzle.yLen;
        xLen = puzzle.xLen;
        breakCount = puzzle.breakCount;

        numberClue = new List<int[,]>();
        numberClue.Add(new int[puzzle.yLen, puzzle.xLen]); // front-back;
        numberClue.Add(new int[puzzle.xLen, puzzle.zLen]); // top-bottom;
        numberClue.Add(new int[puzzle.yLen, puzzle.zLen]); // right-left;

        var originalNumberClue = puzzle.numberClue;

        for (int face = 0; face < 3; ++face)
        {
            for (int i = 0; i < originalNumberClue[face].GetLength(0); ++i)
            {
                for (int j = 0; j < originalNumberClue[face].GetLength(1); ++j)
                {
                    if (originalNumberClue[face][i, j].number == -1)
                    {
                        numberClue[face][i, j] = -1;
                    }
                    else
                    {
                        numberClue[face][i, j] = originalNumberClue[face][i, j].number * 10 + (int)originalNumberClue[face][i, j].shape;
                    }
                }
            }
        }
    }
}
