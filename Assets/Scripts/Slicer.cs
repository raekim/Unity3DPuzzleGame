using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicer : MonoBehaviour
{
    Slicers slicers;

    public enum SLICER_TYPE
    {
        RED,BLUE
    }

    public enum SLICER_POSITION
    {
        FRONT_LEFT,
        FRONT_RIGHT,
        BACK_LEFT,
        BACK_RIGHT,
        MAX
    }

    public SLICER_TYPE slicerType;
    public SLICER_POSITION slicerPosition;

    int dir;    // 슬라이서의 world position 진행 방향에 따라 -1 또는 1 
    int sliceStep;
    int maxStep;
    Vector3 startLocation;        // sliceStep이 0일 때의 slicer 위치
    float cubeWidth = 1f;       // 정육면체 큐브의 폭
    float stepDistance;
    Cube[,,] puzzleCubes;
    int[] puzzleSize;           // 퍼즐의 z,y,x축 크기
    Vector3 clickOffSet;
    float lastClickedTime;

    float minClampValue, maxClampValue;

    private void Awake()
    {
        slicers = GetComponentInParent<Slicers>();
    }

    private void Start()
    {
        // 캡슐 오브젝트의 click, drag 이벤트를 들을 수 있게 설정
        GetComponentInChildren<SlicerCapsule>().capsuleClickDelegate += SlicerClick;
        GetComponentInChildren<SlicerCapsule>().capsuleDragDelegate += SlicerDrag;
        SetSlicerToStartPosition();

        lastClickedTime = Time.time;
    }

    public void InitSlicer(Puzzle puzzle)
    {
        InitSlicerInfo(puzzle);
        SetSlicerToStartPosition();
    }

    void InitSlicerInfo(Puzzle puzzle)
    {
        // 퍼즐에 대한 정보 얻어오기
        puzzleCubes = puzzle.cubes;
        puzzleSize = new int[3] { puzzle.zLen, puzzle.yLen, puzzle.xLen };
        startLocation = puzzleCubes[0, 0, 0].transform.position;

        // 슬라이서의 종류(Red 또는 Blue)와 포지션에 따라 시작 위치와 이동 range를 정한다
        if (slicerType == SLICER_TYPE.RED)
        {
            maxStep = puzzleSize[2] - 1;

            switch (slicerPosition)
            {
                case SLICER_POSITION.FRONT_LEFT:
                    dir = -1;
                    startLocation += cubeWidth * (new Vector3(.5f, -puzzleSize[1] * .5f + .5f, 1f));
                    break;
                case SLICER_POSITION.FRONT_RIGHT:
                    dir = 1;
                    startLocation += cubeWidth * (new Vector3(-puzzleSize[2] + .5f, -puzzleSize[1] * .5f + .5f, 1f));
                    break;
                case SLICER_POSITION.BACK_LEFT:
                    dir = -1;
                    startLocation += cubeWidth * (new Vector3(.5f, -puzzleSize[1] * .5f + .5f, -puzzleSize[0]));
                    break;
                case SLICER_POSITION.BACK_RIGHT:
                    dir = 1;
                    startLocation += cubeWidth * (new Vector3(-puzzleSize[2] + .5f, -puzzleSize[1] * .5f + .5f, -puzzleSize[0]));
                    break;
            }

            // 슬라이서 드래그 좌표의 min, max 설정
            minClampValue = Mathf.Min(startLocation.x, startLocation.x + dir * maxStep * cubeWidth);
            maxClampValue = Mathf.Max(startLocation.x, startLocation.x + dir * maxStep * cubeWidth);
        }
        else if(slicerType == SLICER_TYPE.BLUE)
        {
            maxStep = puzzleSize[0] - 1;

            switch (slicerPosition)
            {
                case SLICER_POSITION.FRONT_LEFT:
                    dir = -1;
                    startLocation += cubeWidth * (new Vector3(1f, -puzzleSize[1] * .5f + .5f, .5f));
                    break;
                case SLICER_POSITION.FRONT_RIGHT:
                    dir = -1;
                    startLocation += cubeWidth * (new Vector3(-puzzleSize[2], -puzzleSize[1] * .5f + .5f, .5f));
                    break;
                case SLICER_POSITION.BACK_LEFT:
                    dir = 1;
                    startLocation += cubeWidth * (new Vector3(1f, -puzzleSize[1] * .5f + .5f, -puzzleSize[0] + .5f));
                    break;
                case SLICER_POSITION.BACK_RIGHT:
                    startLocation += cubeWidth * (new Vector3(-puzzleSize[2], -puzzleSize[1] * .5f + .5f, -puzzleSize[0] + .5f));
                    dir = 1;
                    break;
            }

            // 슬라이서 드래그 좌표의 min, max 설정
            minClampValue = Mathf.Min(startLocation.z, startLocation.z + dir * maxStep * cubeWidth);
            maxClampValue = Mathf.Max(startLocation.z, startLocation.z + dir * maxStep * cubeWidth);
        }

        // 각 step 사이의 거리 계산
        stepDistance = Mathf.Abs(minClampValue - maxClampValue) / maxStep;
    }

    void SetSlicerToStartPosition()
    {
        // 슬라이서를 step이 0일때의 위치로 되돌린다
        transform.position = startLocation;
        ShowAndHideCubes(sliceStep, 0);
        sliceStep = 0;
    }

    void SlicerClick()
    {
        // 현재 마우스를 클릭한 지점과 slicer capsule의 중점 사이의 offset을 구한다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float distance = (transform.position - Camera.main.transform.position).magnitude;

        var rayPoint = ray.GetPoint(distance);

        clickOffSet = transform.position - rayPoint;

        // 더블 클릭 하면 0단계로 돌아옴
        float currentClickedTime = Time.time;

        if(currentClickedTime - lastClickedTime < .4f)
        {
            SetSlicerToStartPosition();
            slicers.registerSlicerAction(this, sliceStep);
        }

        lastClickedTime = currentClickedTime;
    }

    void SlicerDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float distance = (transform.position - Camera.main.transform.position).magnitude;

        var rayPoint = ray.GetPoint(distance);

        if (slicerType == SLICER_TYPE.RED)
        {
            transform.position = new Vector3(rayPoint.x + clickOffSet.x, transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, rayPoint.z + clickOffSet.z);
        } 

        // 슬라이서 범위 clmap하고, slicerStep을 정한다
        transform.position = ClampAndSnapSlicerRange(transform.position);

        // 슬라이서 부모 오브젝트에게 이 슬라이서의 행동을 보고한다
        slicers.registerSlicerAction(this, sliceStep);
    }

    Vector3 ClampAndSnapSlicerRange(Vector3 slicerPosition)
    {
        if(slicerType == SLICER_TYPE.RED)
        {
            // clamp
            slicerPosition.x = Mathf.Clamp(slicerPosition.x, minClampValue, maxClampValue);

            // sliceStep을 정한다
            int newSliceStep = Mathf.FloorToInt(Mathf.Abs(slicerPosition.x - startLocation.x) / stepDistance);

            // snap
            slicerPosition.x = startLocation.x + dir * newSliceStep * cubeWidth;

            // 단면을 숨기거나 보이거나 한다
            ShowAndHideCubes(sliceStep, newSliceStep);
            sliceStep = newSliceStep;
        }
        else if(slicerType == SLICER_TYPE.BLUE)
        {
            // clamp
            slicerPosition.z = Mathf.Clamp(slicerPosition.z, minClampValue, maxClampValue);

            // sliceStep을 정한다
            int newSliceStep = Mathf.FloorToInt(Mathf.Abs(slicerPosition.z - startLocation.z) / stepDistance);

            // snap
            slicerPosition.z = startLocation.z + dir * newSliceStep * cubeWidth;

            // 단면을 숨기거나 보이거나 한다
            ShowAndHideCubes(sliceStep, newSliceStep);
            sliceStep = newSliceStep;
        }

        return slicerPosition;
    }

    void ShowAndHideCubes(int beforeStep, int afterStep)
    {
        if (beforeStep == afterStep) return;

        if (slicerType == SLICER_TYPE.RED)
        {
            int beforeCubeX = 0;
            int afterCubeX = 0;

            switch(slicerPosition)
            {
                case SLICER_POSITION.BACK_LEFT:
                case SLICER_POSITION.FRONT_LEFT:
                    beforeCubeX = beforeStep;
                    afterCubeX = afterStep;
                    break;
                case SLICER_POSITION.BACK_RIGHT:
                case SLICER_POSITION.FRONT_RIGHT:
                    beforeCubeX = puzzleSize[2] - 1 - beforeStep;
                    afterCubeX = puzzleSize[2] - 1 - afterStep;
                    break;
            }

            // 숨기기
            if (beforeStep < afterStep)
            {
                for (int i = beforeCubeX; i != afterCubeX; i -= dir)
                {
                    for (int z = 0; z < puzzleSize[0]; ++z)
                    {
                        for (int y = 0; y < puzzleSize[1]; ++y)
                        {
                            puzzleCubes[z, y, i].gameObject.SetActive(false);
                        }
                    }
                }
            }
            // 보이기
            else if (beforeStep > afterStep)
            {
                for (int i = beforeCubeX; i != afterCubeX + dir; i += dir)
                {
                    for (int z = 0; z < puzzleSize[0]; ++z)
                    {
                        for (int y = 0; y < puzzleSize[1]; ++y)
                        {
                            if (!puzzleCubes[z, y, i].isDestroyed) puzzleCubes[z, y, i].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        else if(slicerType == SLICER_TYPE.BLUE)
        {
            int beforeCubeZ = 0;
            int afterCubeZ = 0;

            switch (slicerPosition)
            {
                case SLICER_POSITION.BACK_LEFT:
                case SLICER_POSITION.BACK_RIGHT:
                    beforeCubeZ = puzzleSize[0] - 1 - beforeStep;
                    afterCubeZ = puzzleSize[0] - 1 - afterStep;
                    break;
                case SLICER_POSITION.FRONT_LEFT:
                case SLICER_POSITION.FRONT_RIGHT:
                    beforeCubeZ = beforeStep; 
                    afterCubeZ = afterStep; 
                    break;
            }

            // 숨기기
            if (beforeStep < afterStep)
            {
                for (int i = beforeCubeZ; i != afterCubeZ; i -= dir)
                {
                    for (int x = 0; x < puzzleSize[2]; ++x)
                    {
                        for (int y = 0; y < puzzleSize[1]; ++y)
                        {
                            puzzleCubes[i, y, x].gameObject.SetActive(false);
                        }
                    }
                }
            }
            else if (beforeStep > afterStep)
            {
                // 보이기
                for (int i = beforeCubeZ; i != afterCubeZ + dir; i += dir)
                {
                    for (int x = 0; x < puzzleSize[2]; ++x)
                    {
                        for (int y = 0; y < puzzleSize[1]; ++y)
                        {
                            if (!puzzleCubes[i, y, x].isDestroyed) puzzleCubes[i, y, x].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
