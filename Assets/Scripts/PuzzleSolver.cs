//#define SolveTheActualPuzzle // 실제 큐브들로 이루어진 퍼즐을 게임화면에서 푼다

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSolver
{
    public enum FACE_TYPE
    {
        FRONT_BACK,
        TOP_BOTTOM,
        RIGHT_LEFT
    }

    class Line
    {
        public List<puzzleIndex> line;      // 실제 라인에 해당하는 큐브 인덱스들
        public Puzzle.NumberClue clue;
        public FACE_TYPE faceType;
        public int priority = 0;
        public int heapIndex;           // 힙에서의 인덱스 (index 1 : 루트노드)
        public bool mark = false;                   // mark된 라인 clue들만 나중에 최종 퍼즐에 남길 것
        public bool isCompleteDescription = false;
        public bool solved = false;
        public bool insideHeap = false;                 // 이 라인이 힙에 들어 가 있는가?
    }

    public class puzzleIndex
    {
        public int z, y, x;    // z : front->back 깊이좌표, y : 위->아래 세로좌표, x : 왼쪽->오른쪽 가로좌표

        public puzzleIndex(int _z, int _y, int _x)
        {
            z = _z;
            y = _y;
            x = _x;
        }
    }

    class lineIndex
    {
        public FACE_TYPE faceType;   // 0 : front->back축을 바라보는 면, 1 : top->bottom축을 바라보는 면, 2 : right->left축을 바라보는 면
        public int i, j;       // i: 위->아래 세로좌표, j : 왼쪽->오른쪽 가로좌표

        public lineIndex(FACE_TYPE _faceType, int _i, int _j)
        {
            faceType = _faceType;
            i = _i;
            j = _j;
        }
    }

    public enum CELLSTATE
    {
        BLANK,
        SOLID,
        EMPTY
    }

    public class MyPuzzle
    {
        public int myBreaks = 0; // 퍼즐을 풀면서 부순 큐브 수 (퍼즐 풀었나 판정에 사용)
        public CELLSTATE[,,] puzzleState;  // "퍼즐"

        public MyPuzzle(Puzzle puzzle)
        {
            puzzleState = new CELLSTATE[puzzle.zLen, puzzle.yLen, puzzle.xLen];  // "퍼즐"

            for (int z = 0; z < puzzle.zLen; ++z)
            {
                for (int y = 0; y < puzzle.yLen; ++y)
                {
                    for (int x = 0; x < puzzle.xLen; ++x)
                    {
                        puzzleState[z, y, x] = CELLSTATE.BLANK;
                    }
                }
            }
        }
    }

    LineSolver3D lineSolver;

    public void ApplyInitialNumberClues(Puzzle puzzle)
    {
        // 아무것도 삭제되지 않은 초기 number clue 만들기
        SetInitialNumberClues(puzzle);
    }

#if SolveTheActualPuzzle
    Cube[,,] puzzleCube;
#endif
    public void ProcessPuzzle(Puzzle puzzle)
    {
        lineSolver = new LineSolver3D();
        lineSolver.Init();

#if SolveTheActualPuzzle
        puzzleCube = puzzle.cubes;
#endif
        // 퍼즐을 풀고 number clue중 필요하지 않은 것 삭제하기
        List<Line[,]> linesOnFaces; // 3면에 존재하는 모든 라인들. 인덱스에 따른 면은 FACE_TYPE 참조
        List<Line> completeLines = new List<Line>();       // complete-description 라인들
        linesOnFaces = InitLines(completeLines, puzzle); // 모든 면의 모든 라인들 구하기

        bool puzzleSolved = SolvePuzzle(puzzle, linesOnFaces, completeLines);

        if (puzzleSolved)
        {
            EraseClues(puzzle, linesOnFaces);
        }
    } 

    bool SolvePuzzle(Puzzle puzzle, List<Line[,]> linesOnFaces, List<Line> completeLines)
    {
        MyPuzzle myPuzzle = new MyPuzzle(puzzle);
        
        LineHeap Q = new LineHeap(puzzle.zLen * puzzle.yLen * puzzle.xLen); // line solver가 사용 할 우선순위 큐

        // 처음 몇 개의 complete-description 라인을 마크하고 풀고, 상태가 바뀐 cell의 수직 라인들 우선순위 올리고 힙에 넣기
        SolveCompleteDescriptionLines(true, 0, linesOnFaces, completeLines, myPuzzle, Q);

        // complete-description 라인들만 빼고 모두 힙에 넣는다
        foreach(var face in linesOnFaces)
        {
            foreach(Line line in face)
            {
                if (!line.isCompleteDescription && !line.insideHeap) Q.Insert(line);
            }
        }

        // 휴리스틱 알고리즘으로 풀기...
        while(myPuzzle.myBreaks != puzzle.breakCount)
        {
            Line lineToSolve = Q.Pop();

            if (lineToSolve == null)
            {
                // 풀다가 안 풀리면 complete-description 라인 하나씩 추가 해 주면서 진행
                if (SolveCompleteDescriptionLines(true, 1, linesOnFaces, completeLines, myPuzzle, Q)) continue;
                else return false;  // 퍼즐 풀이 실패!
            }

            if (isLinePassivelySolved(lineToSolve, myPuzzle))
            {
                lineToSolve.solved = true;  // 이미 풀려있는 라인임
                continue;
            }

            // line solver로 라인을 푼다
            List<CELLSTATE> beforeLineSolver = GetBeforeLine(lineToSolve.line, myPuzzle);
            List<CELLSTATE> afterLineSolver = lineSolver.lineSolver(lineToSolve.clue, lineToSolve.line, myPuzzle);
            
            // line solver 적용 전, 후의 라인을 비교한다
            int lineLen = afterLineSolver.Count;
            List<puzzleIndex> line = lineToSolve.line;

            for (int j = 0; j < lineLen; ++j)
            {
                if (beforeLineSolver[j] != afterLineSolver[j])
                {
                    lineToSolve.mark = true;    // deduction을 하나라도 만들었으므로 라인을 mark한다

                    // cell state 영구적으로 변경하기
                    myPuzzle.puzzleState[line[j].z, line[j].y, line[j].x] = afterLineSolver[j];


                    if (afterLineSolver[j] == CELLSTATE.EMPTY)
                    {
#if SolveTheActualPuzzle
                        puzzleCube[line[j].z, line[j].y, line[j].x].PlayDestoryEffect();
#endif
                        myPuzzle.myBreaks++;
                    }
                    else if (afterLineSolver[j] == CELLSTATE.SOLID)
                    {
#if SolveTheActualPuzzle
                        puzzleCube[line[j].z, line[j].y, line[j].x].isProtected = true;
                        puzzleCube[line[j].z, line[j].y, line[j].x].SetColor(Color.cyan, (int)Cube.MATERIAL_INDEX.BACKGROUND);
                        puzzleCube[line[j].z, line[j].y, line[j].x].PlayProtectAnimation();
#endif
                    }

                    // 수직 라인과 현재 라인의 우선순위 수정
                    Line[] perpendicularLines = GetPerpendicularLines(linesOnFaces, lineToSolve.faceType, line[j]);
                    if (afterLineSolver[j] == CELLSTATE.SOLID)
                    {
                        lineToSolve.priority -= 2;
                        perpendicularLines[0].priority += 2;
                        perpendicularLines[1].priority += 2;
                    }
                    else if (afterLineSolver[j] == CELLSTATE.EMPTY)
                    {
                        lineToSolve.priority -= 1;
                        perpendicularLines[0].priority += 1;
                        perpendicularLines[1].priority += 1;
                    }

                    // 수직 라인들 힙에 넣기
                    for (int index = 0; index < 2; ++index)
                    {
                        if (!perpendicularLines[index].insideHeap)
                        {
                            Q.Insert(perpendicularLines[index]);
                        }
                        else
                        {
                            // 이미 힙에 있다면 float up 해 주기
                            Q.Floatup(perpendicularLines[index].heapIndex);
                        }
                    }
                }
            }

            // line이 풀렸는가?
            if(isLineSolved(lineToSolve, myPuzzle))
            {
                lineToSolve.solved = true;
            }
        }

        // 성공!
        return true;        
    }

    bool isLinePassivelySolved(Line line, MyPuzzle myPuzzle)
    {
        int solids = 0;
        int groups = 0;
        int lineCount = line.line.Count;
        var l = line.line;
        CELLSTATE beforeCell = CELLSTATE.EMPTY;
    
        for (int i = 0; i < lineCount; ++i)
        {
            CELLSTATE currentCell = myPuzzle.puzzleState[l[i].z, l[i].y, l[i].x];
            
            // 모든 solid를 칠하지 않았어도 empty만 맞으면 풀었다고 본다
            if (currentCell == CELLSTATE.SOLID || currentCell == CELLSTATE.BLANK)
            {
                if(beforeCell == CELLSTATE.EMPTY)
                {
                    groups++;
                }
                solids++;
            }
    
            beforeCell = currentCell;
        }
    
        return solids == line.clue.number;  // SOLID가 아닌 큐브는 모두 EMPTY일 것이므로 group은 상관 없이 전체 SOLID 갯수만 보면 된다
    }

    bool isLineSolved(Line line, MyPuzzle myPuzzle)
    {
        int solids = 0;
        int lineCount = line.line.Count;
        var l = line.line;

        for (int i = 0; i < lineCount; ++i)
        {
            CELLSTATE currentCell = myPuzzle.puzzleState[l[i].z, l[i].y, l[i].x];

            // 하나라도 BLANK인 큐브가 있으면 라인 전체가 풀리지 않은 것
            // 즉, 이 라인이 풀린다면 최소 하나의 단서가 추가될 수 있음
            if (currentCell == CELLSTATE.BLANK) return false;

            if (currentCell == CELLSTATE.SOLID)
            {
                solids++;
            }
        }

        return solids == line.clue.number;  // SOLID가 아닌 큐브는 모두 EMPTY일 것이므로 group은 상관 없이 전체 SOLID 갯수만 보면 된다
    }

    Line FindUnSolvedCompleteLine(List<Line> completeLines, MyPuzzle myPuzzle)
    {
        Line lineToSolve = null;

        while (completeLines.Count > 0)
        {
            int idx = Random.Range(0, completeLines.Count);
            lineToSolve = completeLines[idx];

            if (isLineSolved(lineToSolve, myPuzzle))
            {
                lineToSolve.solved = true;
                completeLines.RemoveAt(idx);

                lineToSolve = null;
            }
            else
            {
                break;
            }
        }

        return lineToSolve;
    }

    List<CELLSTATE> GetBeforeLine(List<puzzleIndex> line, MyPuzzle myPuzzle)
    {
        int lineLen = line.Count;

        List<CELLSTATE> res = new List<CELLSTATE>(lineLen);
  
        for (int i = 0; i < lineLen; ++i)
        {
            res.Add(myPuzzle.puzzleState[line[i].z, line[i].y, line[i].x]);
        }

        return res;
    }

    Line[] GetPerpendicularLines(List<Line[,]> linesOnFaces, FACE_TYPE face, puzzleIndex index)
    {
        // FRONT_BACK : [puzzle.yLen, puzzle.xLen]
        // TOP_BOTTOM : [puzzle.xLen, puzzle.zLen]
        // RIGHT_LEFT : [puzzle.yLen, puzzle.zLen]

        Line[] res = new Line[2];

        switch(face)
        {
            case FACE_TYPE.FRONT_BACK:
                res[0] = linesOnFaces[(int)FACE_TYPE.TOP_BOTTOM][index.x, index.z];
                res[1] = linesOnFaces[(int)FACE_TYPE.RIGHT_LEFT][index.y, index.z];
                break;
            case FACE_TYPE.TOP_BOTTOM:
                res[0] = linesOnFaces[(int)FACE_TYPE.FRONT_BACK][index.y, index.x];
                res[1] = linesOnFaces[(int)FACE_TYPE.RIGHT_LEFT][index.y, index.z];
                break;
            case FACE_TYPE.RIGHT_LEFT:
                res[0] = linesOnFaces[(int)FACE_TYPE.FRONT_BACK][index.y, index.x];
                res[1] = linesOnFaces[(int)FACE_TYPE.TOP_BOTTOM][index.x, index.z];
                break;
        }

        return res;
    }

    bool SolveCompleteDescriptionLines(bool firstCall, int num, List<Line[,]> linesOnFaces, List<Line> completeLines, MyPuzzle myPuzzle, LineHeap Q)
    {
        // 랜덤한 num개의 complete-description 라인을 푼다
        for (int i = 0; i < Mathf.Min(num, completeLines.Count); ++i)
        {
            // solved 아닌 라인을 찾는다
            Line lineToSolve = FindUnSolvedCompleteLine(completeLines, myPuzzle);
            if (lineToSolve == null)
            {
                return firstCall;  // unsolved인 complete-description이 하나도 없음. first call이 아니라면 false 반환
            }

            // line solver로 라인을 푼다
            List<CELLSTATE> beforeLineSolver = GetBeforeLine(lineToSolve.line, myPuzzle);
            List<CELLSTATE> afterLineSolver = lineSolver.lineSolver(lineToSolve.clue, lineToSolve.line, myPuzzle);
            lineToSolve.mark = true;
            lineToSolve.solved = true;

            // line solver 적용 전, 후의 라인을 비교한다
            int lineLen = afterLineSolver.Count;
            List<puzzleIndex> line = lineToSolve.line;

            for (int j = 0; j < lineLen; ++j)
            {
                if(beforeLineSolver[j] != afterLineSolver[j])
                {
                    // cell state 영구적으로 변경하기
                    myPuzzle.puzzleState[line[j].z, line[j].y, line[j].x] = afterLineSolver[j];


                    if (afterLineSolver[j] == CELLSTATE.EMPTY)
                    {
#if SolveTheActualPuzzle
                        puzzleCube[line[j].z, line[j].y, line[j].x].PlayDestoryEffect();
#endif
                        myPuzzle.myBreaks++;
                    }
                    else if (afterLineSolver[j] == CELLSTATE.SOLID)
                    {
#if SolveTheActualPuzzle
                        puzzleCube[line[j].z, line[j].y, line[j].x].isProtected = true;
                        puzzleCube[line[j].z, line[j].y, line[j].x].SetColor(Color.cyan, (int)Cube.MATERIAL_INDEX.BACKGROUND);
                        puzzleCube[line[j].z, line[j].y, line[j].x].PlayProtectAnimation();
#endif
                    }

                    // 수직 라인의 우선순위 높이기
                    Line[] perpendicularLines = GetPerpendicularLines(linesOnFaces, lineToSolve.faceType, line[j]);
                    if(afterLineSolver[j] == CELLSTATE.SOLID)
                    {
                        perpendicularLines[0].priority += 2;
                        perpendicularLines[1].priority += 2;
                    }
                    else if(afterLineSolver[j] == CELLSTATE.EMPTY)
                    {
                        perpendicularLines[0].priority += 1;
                        perpendicularLines[1].priority += 1;
                    }

                    // 수직 라인들 힙에 넣기
                    for (int index = 0; index < 2; ++index)
                    {
                        if(!perpendicularLines[index].insideHeap)
                        {
                            Q.Insert(perpendicularLines[index]);
                        }
                    }
                }
            }
        }

        return true;
    }

    List<Line[,]> InitLines(List<Line> completeLines, Puzzle puzzle)
    {
        List<Line[,]> res = new List<Line[,]>();

        res.Add(new Line[puzzle.yLen, puzzle.xLen]);
        res.Add(new Line[puzzle.xLen, puzzle.zLen]);
        res.Add(new Line[puzzle.yLen, puzzle.zLen]);

        for (int face = 0; face < 3; ++face)
        {
            // 퍼즐의 한 면
            var numberClueFace = puzzle.numberClue[face];

            for (int i = 0; i < numberClueFace.GetLength(0); ++i)
            {
                for (int j = 0; j < numberClueFace.GetLength(1); ++j)
                {
                    var numberClue = numberClueFace[i, j]; // 해당 라인의 number clue
                    var line = GetPuzzleLine(new lineIndex((FACE_TYPE)face, i, j), puzzle);

                    // 라인 만들어서 넣기
                    Line newLine = new Line();
                    newLine.line = line;
                    newLine.clue = numberClue;
                    newLine.isCompleteDescription = IsCompleteDescription(newLine); 
                    newLine.faceType = (FACE_TYPE)face;

                    // complete-description 라인들은 따로 관리
                    if (newLine.isCompleteDescription)
                    {
                        completeLines.Add(newLine);
                    }

                    res[face][i, j] = newLine;
                }
            }
        }

        return res;
    }

    bool IsCompleteDescription(Line L)
    {
        int lineLen = L.line.Count;
        int solids = L.clue.number;

        if (solids == 0) return true;   // number clue가 0인 라인

        switch(L.clue.shape)
        {
            case Puzzle.CLUE_SHAPE.NONE:
                if (solids == lineLen)
                {
                    return true;
                }
                break;
            case Puzzle.CLUE_SHAPE.CIRCLE:
                if (solids == 2 && lineLen == 3)
                {
                    return true;
                }
                break;
            case Puzzle.CLUE_SHAPE.SQUARE:
                if (solids == 3 && lineLen == 5)
                {
                    return true;
                }
                break;
        }
        
        return false;
    }

    void EraseClues(Puzzle puzzle, List<Line[,]> linesOnFaces)
    {
        // 필요없는 clue 지우기
        for (int face = 0; face < 3; ++face)
        {
            for (int i = 0; i < linesOnFaces[face].GetLength(0); ++i)
            {
                for (int j = 0; j < linesOnFaces[face].GetLength(1); ++j)
                {
                    Line L = linesOnFaces[face][i, j];
                    if (!L.mark)
                    {
                        // mark되지 않은 라인 : 퍼즐 풀이에 쓰이지 않은 라인
                        var line = L.line;
                        foreach (puzzleIndex P in line)
                        {
                            // PuzzleData로 저장할 때 이 라인은 빼고 저장하도록 number clue의 숫자를 -1로 설정.
                            puzzle.numberClue[face][i, j].number = -1;
                            puzzle.cubes[P.z, P.y, P.x].HideNumberClueOnCertainFace(L.faceType);
                        }
                    }
                }
            }
        }
    }

    void SetInitialNumberClues(Puzzle puzzle)
    {
        for (int face = 0; face < 3; ++face)
        {
            // 퍼즐의 한 면
            var numberClueFace = puzzle.numberClue[face];

            // 한 면의 모든 라인에 대하여
            for (int i = 0; i < numberClueFace.GetLength(0); ++i)
            {
                for (int j = 0; j < numberClueFace.GetLength(1); ++j)
                {
                    // 해당 라인의 number clue 계산
                    var line = GetPuzzleLine(new lineIndex((FACE_TYPE)face, i, j), puzzle);
                    var numberClue = CalculateNumberClue(line, puzzle);

                    // 퍼즐에 기록
                    Puzzle.CLUE_SHAPE clueShape;

                    switch(numberClue[1])
                    {
                        case 1:
                        case 0:
                            clueShape = Puzzle.CLUE_SHAPE.NONE;
                            break;
                        case 2:
                            clueShape = Puzzle.CLUE_SHAPE.CIRCLE;
                            break;
                        default:
                            clueShape = Puzzle.CLUE_SHAPE.SQUARE;
                            break;
                    }

                    numberClueFace[i, j] = new Puzzle.NumberClue(numberClue[0], clueShape);
                }
            }
        }
    }

    private List<puzzleIndex> GetPuzzleLine(lineIndex index, Puzzle puzzle)
    {
        List<puzzleIndex> res = new List<puzzleIndex>();

        switch (index.faceType)
        {
            case FACE_TYPE.FRONT_BACK: // front->back축을 바라보는 면
                for (int z = 0; z < puzzle.zLen; ++z)
                {
                    res.Add(new puzzleIndex(z, index.i, index.j));
                }
                break;
            case FACE_TYPE.TOP_BOTTOM: // top->bottom축을 바라보는 면
                for (int y = 0; y < puzzle.yLen; ++y)
                {
                    res.Add(new puzzleIndex(index.j, y, index.i));
                }
                break;
            case FACE_TYPE.RIGHT_LEFT: // right->left축을 바라보는 면
                for (int x = 0; x < puzzle.xLen; ++x)
                {
                    res.Add(new puzzleIndex(index.j, index.i, x));
                }
                break;
        }

        return res;
    }

    int[] CalculateNumberClue(List<puzzleIndex> line, Puzzle puzzle)
    {
        int[] res = new int[2]; // res[0] : solid한 조각 갯수, res[1] : solid 조각 그룹 수
        res[0] = res[1] = 0;

        int before = -1;

        foreach (puzzleIndex cube in line)
        {
            int currentCubeState = puzzle.answerArray[cube.z, cube.y, cube.x];  // 1 : solid, 0 : empty
            if (currentCubeState == 1)    
            {
                // solid
                res[0]++;
            }
            else     
            {
                // empty
                if (before == 1) res[1]++;
            }

            before = currentCubeState;
        }

        if (before == 1) res[1]++;

        return res;
    }

    // Line 힙
    class LineHeap
    {
        int size = 0;
        List<Line> heap;

        public LineHeap(int capacity)
        {
            heap = new List<Line>(capacity + 1);
            heap.Add(null); // 루트 노드의 인덱스가 1이 되도록 하기 위한 더미
        }

        public Line Pop()
        {
            Line result = null;

            if(size > 0)
            {
                // 루트 노드 인덱스는 1이다
                result = heap[1];
                result.insideHeap = false;
                
                if(size > 1)
                {
                    heap[1] = heap[size];
                    heap[1].heapIndex = 1;
                    FloatDown(1);
                }

                size--;
            }

            return result;
        }

        public void Insert(Line line)
        {
            size++;
            line.insideHeap = true;
            line.heapIndex = size;

            if(heap.Count <= size)
            {
                heap.Add(line);
            }
            else
            {
                heap[size] = line;
            }

            Floatup(size);
        }

        public void Floatup(int index)
        {
            while(index > 1 && heap[parent(index)].priority < heap[index].priority)
            {
                // 부모 노드와 현재 노드 swap
                Line currentLine = heap[index];
                Line parentLine = heap[parent(index)];

                heap[index] = parentLine;
                parentLine.heapIndex = index;

                heap[parent(index)] = currentLine;
                currentLine.heapIndex = parent(index);

                index = parent(index);
            }
        }

        public void FloatDown(int index)
        {
            int leftChild = left(index);
            int rightChild = right(index);
            int highestPriority = index;

            if (leftChild <= size && heap[leftChild].priority > heap[highestPriority].priority)
            {
                highestPriority = leftChild;
            }

            if (rightChild <= size && heap[rightChild].priority > heap[highestPriority].priority)
            {
                highestPriority = rightChild;
            }

            if (highestPriority != index)
            {
                // 자식노드 중 우선순위가 높은 노드와 현재 노드 swap
                Line currentLine = heap[index];
                Line highestPriorityLine = heap[highestPriority];

                heap[index] = highestPriorityLine;
                highestPriorityLine.heapIndex = index;

                heap[highestPriority] = currentLine;
                currentLine.heapIndex = highestPriority;

                FloatDown(highestPriority);
            }
        }

        int parent(int index)
        {
            // 부모 노드
            return index / 2;
        }

        int left(int index)
        {
            // 왼쪽 자식 노드
            return index * 2;
        }

        int right(int index)
        {
            // 오른쪽 자식 노드
            return index * 2 + 1;
        }
    }
}
