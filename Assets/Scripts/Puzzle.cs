using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle
{
    public string puzzleName;// = "Sofa";
    public int zLen;
    public int yLen;
    public int xLen;
    public int breakCount;      // 부숴야 하는 블록의 총 갯수;
    public List<Color32> answerColors
    = new List<Color32> { new Color32(195,195,195,255), new Color32(185, 122, 87, 255), new Color32(255, 174, 201, 255),
    new Color32(255, 201, 14, 255), new Color32(239, 228, 176, 255), new Color32(181, 230, 29, 255), new Color32(153, 217, 234, 255),
    new Color32(112, 146, 190, 255), new Color32(200, 191, 231, 255)};

    // 1 : 큐브 있음, 2 : 큐브 없음
    public int[,,] answerArray;

    // 표현하고자 하는 색상이 담겨있는 puzzleColors의 인덱스를 저장
    // -1는 없는 큐브임
    public int[,,] answerColorIndex;
    //= new int[,,]
    //{
    //    { {-1,-1,-1,-1},
    //      {0,-1,-1,0},
    //      {0,0,0,0} },
    //    { {-1,-1,-1,-1},
    //      {0,-1,-1,0},
    //      {0,0,0,0} },
    //    { {-1,1,1,-1},
    //      {1,1,1,1},
    //      {1,1,1,1} }
    //};

    public enum CLUE_SHAPE
    {
        NONE, CIRCLE, SQUARE
    }

    public class NumberClue
    {
        public int number; // -1이라면 number clue가 존재하지 않음
        public CLUE_SHAPE shape;

        public NumberClue()
        {
            number = -1;
        }

        public NumberClue(int _number, CLUE_SHAPE _shape)
        {
            number = _number;
            shape = _shape;
        }
    }

    // numberClue[0] : FRONT_BACK 면
    // numberClue[1] : TOP_BOTTOM 면
    // numberClue[2] : RIGHT_LEFT 면
    public List<NumberClue[,]> numberClue;

    public Cube[,,] cubes;

    public void Init()
    {
        zLen = answerArray.GetLength(0);
        yLen = answerArray.GetLength(1);
        xLen = answerArray.GetLength(2);
        
        breakCount = countBlanksInPuzzle();
        
        numberClue = new List<NumberClue[,]>();
        numberClue.Add(new NumberClue[yLen, xLen]); // front-back;
        numberClue.Add(new NumberClue[xLen, zLen]); // top-bottom;
        numberClue.Add(new NumberClue[yLen, zLen]); // right-left;
    }

    int countBlanksInPuzzle()
    {
        int res = 0;

        for (int z = 0; z < zLen; ++z)
        {
            for (int y = 0; y < yLen; ++y)
            {
                for (int x = 0; x < xLen; ++x)
                {
                    if (answerArray[z, y, x] == 0)
                    {
                        res++;
                    }
                }
            }
        }

        return res;
    }
}
