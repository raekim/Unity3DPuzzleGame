using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSolver3D
{
    class Block
    {
        public int start, end;

        public Block(int _start, int _end)
        {
            start = _start;
            end = _end;
        }

        public Block()
        {
            start = -1;
            end = -1;
        }
    };

    List<List<int>>[,] expansionCache = new List<List<int>>[3, 10];  // expansionCache[Puzzle.CLUE_SHAPE, int(clue의 숫자)] = List<List<int>>

    public void Init()
    {
        // expansion 캐싱
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    expansionCache[i, j] = new List<List<int>>();
                }
            }

            for (int i = 1; i <= 9; ++i)
            {
                // group 1개짜리
                List<int> list = new List<int>();
                list.Add(i);
                expansionCache[0, i].Add(list);
            }

            for (int i = 2; i <= 9; ++i)
            {
                // group 2개짜리
                for (int j = 1; j <= i - 1; ++j)
                {
                    List<int> list = new List<int>();
                    list.Add(j);
                    list.Add(i - j);
                    expansionCache[1, i].Add(list);
                }
            }
        }
    }

    // 주로 사용될 line solver 함수... 나머지 함수들은 거들 뿐
    public List<PuzzleSolver.CELLSTATE> lineSolver(Puzzle.NumberClue clue, List<PuzzleSolver.puzzleIndex> line, PuzzleSolver.MyPuzzle myPuzzle)
    {
        int lineLen = line.Count;
        List<PuzzleSolver.CELLSTATE> result = new List<PuzzleSolver.CELLSTATE>(lineLen);
        if (clue.number == 0)
        {
            for (int i = 0; i < lineLen; ++i)
            {
                result.Add(PuzzleSolver.CELLSTATE.EMPTY);
            }
            return result;
        }

        for (int i = 0; i < lineLen; ++i)
        {
            result.Add(PuzzleSolver.CELLSTATE.BLANK);
        }

        List<List<PuzzleSolver.CELLSTATE>> results = new List<List<PuzzleSolver.CELLSTATE>>();

        List<List<int>> numberClueExpansions = expansionCache[(int)clue.shape, clue.number];

        // leftMost와 rightMost를 구할 수 있는 모든 expansion들에 대해 풀어보고 모든 풀이의 공통점만 찾아서 반환하기
        foreach (List<int> numberClue in numberClueExpansions)
        {
            List<Block> leftMost = new List<Block>();
            List<Block> rightMost = new List<Block>();

            leftMost = getLeftMostSolution(numberClue, line, true, myPuzzle);
            rightMost = getLeftMostSolution(numberClue, line, false, myPuzzle);

            if(leftMost.Count > 0 && rightMost.Count > 0)
            {
                List<PuzzleSolver.CELLSTATE> res = new List<PuzzleSolver.CELLSTATE>(lineLen);

                for (int i = 0; i < lineLen; ++i)
                {
                    res.Add(myPuzzle.puzzleState[line[i].z, line[i].y, line[i].x]);
                }

                int blockLen = leftMost.Count;

                // 확실한 SOLID 결정
                for (int i = 0; i < blockLen; ++i)
                {
                    if (leftMost[i].end >= rightMost[i].start && leftMost[i].start <= rightMost[i].end)
                    {
                        int overlapCnt = leftMost[i].end - rightMost[i].start + 1;
                        for (int j = 0; j < overlapCnt; ++j)
                        {
                            res[rightMost[i].start + j] = PuzzleSolver.CELLSTATE.SOLID;
                        }
                    }
                }

                // 확실한 EMPTY 결정
                for (int cell = 0; cell < lineLen; ++cell)
                {
                    bool empty = true;

                    for (int i = 0; i < blockLen; ++i)
                    {
                        if (leftMost[i].start <= cell && cell <= rightMost[i].end)
                        {
                            empty = false;
                            break;
                        }
                    }

                    if (empty) res[cell] = PuzzleSolver.CELLSTATE.EMPTY;
                }

                results.Add(res);
            }
        }

        // 모든 result들을 대조해서 공통된 부분만 뽑아내기
        int resultCount = results.Count;

        for (int i = 0; i < lineLen; ++i)
        {
            bool consistent = true;

            for (int j = 0; j < resultCount - 1; ++j)
            {
                if(results[j][i] != results[j+1][i])
                {
                    consistent = false;
                    break;
                }
            }

            if(consistent)
            {
                result[i] = results[0][i];
            }
        }

        return result;
    }

    void GetSimpleLeftMost(List<Block> res, List<int> clue, List<PuzzleSolver.puzzleIndex> line, bool leftMost, int lineLen, int clueCnt, PuzzleSolver.MyPuzzle myPuzzle)
    {
        // simple left-most를 구한다
        int currIdx = 0;
        int startIdx, endIdx;

        for (int i = 0; i < clueCnt; ++i)
        {
            // 블록의 start 정하기
            //for (startIdx = currIdx; startIdx + clue[i] - 1 < lineLen; ++startIdx)
            bool placedBlock = false;

            for (startIdx = currIdx; startIdx < lineLen; ++startIdx)
            {
                bool canPlaceBlock = true;
    
                // 블록의 end 정하기
                for (endIdx = startIdx; endIdx < startIdx + clue[i]; ++endIdx)
                {
                    if (myPuzzle.puzzleState[line[endIdx].z, line[endIdx].y, line[endIdx].x] == PuzzleSolver.CELLSTATE.EMPTY)
                    {
                        canPlaceBlock = false;
                        break;
                    }
                }
    
                endIdx--;
    
                // 놓은 블록의 바로 다음 칸에 SOLID가 있으면 안 됨
                if (endIdx + 1 < lineLen)
                {
                    canPlaceBlock &= myPuzzle.puzzleState[line[endIdx + 1].z, line[endIdx + 1].y, line[endIdx + 1].x] != PuzzleSolver.CELLSTATE.SOLID;
                }
    
                if (canPlaceBlock)
                {
                    res.Add(new Block( startIdx, endIdx ));
                    currIdx = endIdx + 2;
                    placedBlock = true;
                    break;
                }
            }

            // simple left-most 찾기 실패
            if (!placedBlock)
            {
                res.Clear();
                return;
            }
        }
    }

    bool isThereUnassignedSolid(Block rightMostUnassignedSolid, int lineLen, List<PuzzleSolver.puzzleIndex> line, List<Block> leftMost, PuzzleSolver.MyPuzzle myPuzzle)
    {
        List<int> tempLine = new List<int>(lineLen);
        for (int i = 0; i < lineLen; ++i) tempLine.Add(0);

        // tempLine에 leftMost를 그린다
        foreach (Block b in leftMost)
        {
            for (int i = b.start; i <= b.end; ++i)
            {
                tempLine[i] = 1;
            }
        }

        // board와 비교해서 unassigned solid를 찾아낸다
        for (int i = 0; i < lineLen; ++i)
        {
            if (myPuzzle.puzzleState[line[i].z, line[i].y, line[i].x] == PuzzleSolver.CELLSTATE.SOLID && tempLine[i] == 0)
            {
                int solidStart = i;
                int solidEnd = i;

                while (i + 1 < lineLen && myPuzzle.puzzleState[line[i + 1].z, line[i + 1].y, line[i + 1].x] == PuzzleSolver.CELLSTATE.SOLID)
                {
                    solidEnd = ++i;
                }

                rightMostUnassignedSolid.start = solidStart;
                rightMostUnassignedSolid.end = solidEnd;

                return true;
            }
        }

        return false;
    }

    void AlignWithKnownStates(int lineLen, List<PuzzleSolver.puzzleIndex> line, List<Block> res, PuzzleSolver.MyPuzzle myPuzzle)
    {
        if (res.Count == 0) return;

        Block rightMostUnassignedSolid = new Block();

        while (isThereUnassignedSolid(rightMostUnassignedSolid, lineLen, line, res, myPuzzle))
        {
            int blockIdx = -1;

            // unassigned solid의 왼쪽 첫 번째 block 구한다
            for (int i = res.Count - 1; i >= 0; --i)
            {
                if (rightMostUnassignedSolid.start > res[i].end)
                {
                    blockIdx = i;
                    break;
                }
            }

            // 실패!
            if (blockIdx == -1)
            {
                res.Clear();
                return;
            }

            Block block = res[blockIdx];

            bool alignSuccess = false;

            while (blockIdx >= 0)
            {
                // 블록이 unassigned solid 를 포함할 수 있어야 함
                int blockLength = block.end - block.start + 1;
                int unassignedLength = rightMostUnassignedSolid.end - rightMostUnassignedSolid.start + 1;
                
                if (blockLength < unassignedLength)
                {
                    --blockIdx;
                    continue;
                }

                // unassigned와 block이 겹치게 만든다
                {
                    int blockLen = block.end - block.start + 1;
                    block.end = rightMostUnassignedSolid.end;
                    block.start = block.end - blockLen + 1;
                }

                bool isValidPos = true;

                // shift 'block' to valid position
                while (block.end < lineLen && block.start <= rightMostUnassignedSolid.start)
                {
                    isValidPos = true;

                    // 블록의 바로 다음 칸이 SOLID 이면 안 됨
                    if (block.end + 1 < lineLen &&
                        myPuzzle.puzzleState[line[block.end + 1].z, line[block.end + 1].y, line[block.end + 1].x] == PuzzleSolver.CELLSTATE.SOLID)
                    {
                        isValidPos = false;
                    }
                    else
                    {
                        // 블록이 EMPTY 인 칸을 하나라도 차지하면 안 됨
                        for (int i = block.start; i <= block.end; ++i)
                        {
                            if (myPuzzle.puzzleState[line[i].z, line[i].y, line[i].x] == PuzzleSolver.CELLSTATE.EMPTY)
                            {
                                isValidPos = false;
                                break;
                            }
                        }
                    }

                    if (!isValidPos)
                    {
                        block.start++;
                        block.end++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (isValidPos)
                {
                    // re-position blocks right of 'block'
                    int idx = block.end + 2;    // re-position 될 블록들의 시작 인덱스

                    for (int i = blockIdx + 1; i < res.Count; ++i)
                    {
                        bool foundPlace = true;
                        int blockLen = res[i].end - res[i].start + 1;
                        int j = 0;

                        // 블록이 EMPTY 공간을 차지하면 안 됨
                        for (j = idx; j <= idx + blockLen - 1; ++j)
                        {
                            if (j >= lineLen || myPuzzle.puzzleState[line[j].z, line[j].y, line[j].x]  == PuzzleSolver.CELLSTATE.EMPTY)
                            {
                                foundPlace = false;
                                break;
                            }
                        }

                        // 바로 뒤에 SOLID 가 오면 안 됨
                        if (j < lineLen && myPuzzle.puzzleState[line[j].z, line[j].y, line[j].x] == PuzzleSolver.CELLSTATE.SOLID)
                        {
                            foundPlace = false;
                        }

                        if (foundPlace)
                        {
                            res[i].end = j - 1;
                            res[i].start = res[i].end - blockLen + 1;
                            idx = j + 1;
                        }
                        else
                        {
                            // 다음 블록으로 넘어가지 말고 i번째 블록에 대해 다시 try
                            --i;
                            idx++;

                            // 실패!
                            if(idx >= lineLen)
                            {
                                res.Clear();
                                return;
                            }
                        }
                    }

                    // unassigned-solid 에 대해 align 성공
                    alignSuccess = true;
                    break;
                }
                else
                {
                    // try the next block left of 'block'
                    blockIdx--;
                }
            }

            if(!alignSuccess)
            {
                res.Clear();
                return;
            }
        }
    }

    List<Block> getLeftMostSolution(List<int> clue, List<PuzzleSolver.puzzleIndex> line, bool leftMost, PuzzleSolver.MyPuzzle myPuzzle)
    {
        // 만약 solution이 존재하지 않으면 res.Count == 0 인것을 반환
        List<Block> res = new List<Block>();

        int lineLen = line.Count;
        int clueCnt = clue.Count;

        // right-most를 구하는 경우(flip)
        if (!leftMost)
        {
            clue.Reverse();
            line.Reverse();
        }

        // simple left-most를 구한다
        GetSimpleLeftMost(res, clue, line, leftMost, lineLen, clueCnt, myPuzzle);
        
        //// known state와 일치하도록 조절
        AlignWithKnownStates(lineLen, line, res, myPuzzle);

        // right-most를 구하는 경우(flip)
        if (!leftMost)
        {
            res.Reverse();

            foreach (Block b in res)
            {
                b.start = lineLen - 1 - b.start;
                b.end = lineLen - 1 - b.end;

                // swap(b.start, b.end)
                int bStart = b.start;
                b.start = b.end;
                b.end = bStart;
            }

            // clue, line 벡터들은 레퍼런스로 받아왔으므로 다시 뒤집어준다
            clue.Reverse();
            line.Reverse();
        }

        return res;
    }
}
