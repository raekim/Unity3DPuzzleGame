using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// x축 방향으로만 드래그 되는 슬라이서
public class Slicer : MonoBehaviour
{
    public enum SLICER_TYPE
    {
        RED,BLUE
    }

    public enum SLICER_POSITION
    {
        FRONT_LEFT,
        FRONT_RIGHT,
        BACK_LEFT,
        BACK_RIGHT
    }

    public SLICER_TYPE slicerType;
    public SLICER_POSITION slicerPosition;

    public PuzzleManager puzzleManager;

    int sliceStep;
    int maxStep;
    Vector3 startLocation;        // sliceStep이 0일 때의 slicer 위치
    float cubeWidth = 1f;       // 정육면체 큐브의 폭
    float stepDistance;
    Cube[,,] puzzleCubes;
    int[] puzzleSize;           // 퍼즐의 z,y,x축 크기
    Vector3 clickOffSet;

    float minClampValue, maxClampValue;

    private void Start()
    {
        // 캡슐 오브젝트의 click, drag 이벤트를 들을 수 있게 설정
        GetComponentInChildren<SlicerCapsule>().capsuleClickDelegate += SlicerClick;
        GetComponentInChildren<SlicerCapsule>().capsuleDragDelegate += SlicerDrag;

        // 퍼즐에 대한 정보 얻어오기
        puzzleCubes = puzzleManager.GetPuzzleCubes();
        puzzleSize = puzzleManager.GetPuzzleSize();

        // startLocation 과 이동 range 계산
        InitSlicerInfo();

        SetSlicerToStartPosition();
    }

    void InitSlicerInfo()
    {
        // 슬라이서의 종류(Red 또는 Blue)와 포지션에 따라 시작 위치와 이동 range를 정한다
        if(slicerType == SLICER_TYPE.RED)
        {
            maxStep = puzzleSize[2] - 1;

            switch (slicerPosition)
            {
                case SLICER_POSITION.FRONT_LEFT:
                    break;
                case SLICER_POSITION.FRONT_RIGHT:
                    break;
                case SLICER_POSITION.BACK_LEFT:
                    startLocation = puzzleCubes[0, 0, 0].transform.position;
                    startLocation += cubeWidth * (new Vector3(.5f, -puzzleSize[1] * .5f + .5f, -puzzleSize[0]));

                    minClampValue = startLocation.x - maxStep * cubeWidth;
                    maxClampValue = startLocation.x;
                    break;
                case SLICER_POSITION.BACK_RIGHT:
                    break;
            }

            // 각 step 사이의 거리 계산
            stepDistance = Mathf.Abs(startLocation.x - (startLocation.x - maxStep * cubeWidth)) / maxStep;
        }
        else if(slicerType == SLICER_TYPE.BLUE)
        {
            maxStep = puzzleSize[0] - 1;

            switch (slicerPosition)
            {
                case SLICER_POSITION.FRONT_LEFT:
                    break;
                case SLICER_POSITION.FRONT_RIGHT:
                    startLocation = puzzleCubes[0, 0, 0].transform.position;
                    startLocation += cubeWidth * (new Vector3(-puzzleSize[2], 0f, 0f));


                    break;
                case SLICER_POSITION.BACK_LEFT:
                    startLocation = puzzleCubes[0, 0, 0].transform.position;
                    startLocation += cubeWidth * (new Vector3(1f, -puzzleSize[1] * .5f + .5f, -puzzleSize[0] + .5f));

                    minClampValue = startLocation.z;
                    maxClampValue = startLocation.z + maxStep * cubeWidth;
                    break;
                case SLICER_POSITION.BACK_RIGHT:
                    break;
            }

            // 각 step 사이의 거리 계산
            stepDistance = Mathf.Abs(startLocation.z - (startLocation.z + maxStep * cubeWidth)) / maxStep;
        } 
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
            slicerPosition.x = startLocation.x - newSliceStep * cubeWidth;

            // 단면을 숨기거나 보이거나 한다
            ShowAndHideCubes(sliceStep, newSliceStep);
            sliceStep = newSliceStep;
        }
        else
        {
            // clamp
            slicerPosition.z = Mathf.Clamp(slicerPosition.z, minClampValue, maxClampValue);

            // sliceStep을 정한다
            int newSliceStep = Mathf.FloorToInt(Mathf.Abs(slicerPosition.z - startLocation.z) / stepDistance);

            // snap
            slicerPosition.z = startLocation.z + newSliceStep * cubeWidth;

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
            switch(slicerPosition)
            {
                case SLICER_POSITION.BACK_LEFT:
                case SLICER_POSITION.FRONT_LEFT:
                    {
                        // 숨기기
                        if (beforeStep < afterStep)
                        {
                            for (int i = beforeStep; i < afterStep; ++i)
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
                        else if(beforeStep > afterStep)
                        {
                            // 보이기
                            for (int i = beforeStep - 1; i >= afterStep; --i)
                            {
                                for (int z = 0; z < puzzleSize[0]; ++z)
                                {
                                    for (int y = 0; y < puzzleSize[1]; ++y)
                                    {
                                        if (!puzzleCubes[z, y, i].isDestroyed)
                                            puzzleCubes[z, y, i].gameObject.SetActive(true);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SLICER_POSITION.BACK_RIGHT:
                case SLICER_POSITION.FRONT_RIGHT:
                    break;
            }
        }
        else if(slicerType == SLICER_TYPE.BLUE)
        {
            switch (slicerPosition)
            {
                case SLICER_POSITION.BACK_LEFT:
                case SLICER_POSITION.FRONT_RIGHT:
                    {
                        // 숨기기
                        if (beforeStep < afterStep)
                        {
                            for (int i = puzzleSize[0] - afterStep; i < puzzleSize[0] - beforeStep; ++i)
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
                            for (int i = puzzleSize[0] - beforeStep; i < puzzleSize[0] - afterStep; ++i)
                            {
                                for (int x = 0; x < puzzleSize[2]; ++x)
                                {
                                    for (int y = 0; y < puzzleSize[1]; ++y)
                                    {
                                        if (!puzzleCubes[i, y, x].isDestroyed)
                                            puzzleCubes[i, y, x].gameObject.SetActive(true);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SLICER_POSITION.BACK_RIGHT:
                case SLICER_POSITION.FRONT_LEFT:
                
                    break;
            }
        }
    }
}
