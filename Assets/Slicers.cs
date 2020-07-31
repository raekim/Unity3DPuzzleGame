using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicers : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    List<Slicer> redSlicers = new List<Slicer>();
    List<Slicer> blueSlicers = new List<Slicer>();

    private void Awake()
    {
        // puzzleManager 에 모든 slicer들 넘겨주기
        Slicer[] childSlicers = GetComponentsInChildren<Slicer>();
        puzzleManager.SetSlicers(childSlicers);

        // red/blue slicer 따로 저장
        foreach(Slicer slicer in childSlicers)
        {
            switch(slicer.slicerType)
            {
                case Slicer.SLICER_TYPE.RED:
                    redSlicers.Add(slicer);
                    break;
                case Slicer.SLICER_TYPE.BLUE:
                    blueSlicers.Add(slicer);
                    break;
            }
        }
    }

    public void UpdateBlueSlicersVisibility(CameraRotationManager.CAMERA_LOCATION currentCameraLoaction)
    {
        // 카메라가 바라보는 위치에 따라 어떤 blue slicer를 보이게 할 것인지 결정
        Slicer.SLICER_POSITION visibleBluePosition = Slicer.SLICER_POSITION.MAX;

        switch (currentCameraLoaction)
        {
            case CameraRotationManager.CAMERA_LOCATION.BACK_LEFT:
                visibleBluePosition = Slicer.SLICER_POSITION.BACK_RIGHT;
                break;
            case CameraRotationManager.CAMERA_LOCATION.BACK_RIGHT:
                visibleBluePosition = Slicer.SLICER_POSITION.BACK_LEFT;
                break;
            case CameraRotationManager.CAMERA_LOCATION.FRONT_LEFT:
                visibleBluePosition = Slicer.SLICER_POSITION.FRONT_RIGHT;
                break;
            case CameraRotationManager.CAMERA_LOCATION.FRONT_RIGHT:
                visibleBluePosition = Slicer.SLICER_POSITION.FRONT_LEFT;
                break;
        }

        // 적절한 blue slicer 하나만 보이게 하고 나머지는 다 숨긴다
        foreach (Slicer slicer in blueSlicers)
        {
            slicer.gameObject.SetActive(false);
            if (slicer.slicerPosition == visibleBluePosition) slicer.gameObject.SetActive(true);
        }

        UpdateRedSlicersVisibility(visibleBluePosition);
    }

    void UpdateRedSlicersVisibility(Slicer.SLICER_POSITION visibleBluePosition)
    {
        // 현재 보이고 있는 blue slicer에 대응하는 red slicer를 보이게 한다
        Slicer.SLICER_POSITION visibleRedPosition = Slicer.SLICER_POSITION.MAX;

        switch(visibleBluePosition)
        {
            case Slicer.SLICER_POSITION.BACK_LEFT:
                visibleRedPosition = Slicer.SLICER_POSITION.FRONT_RIGHT;
                break;
            case Slicer.SLICER_POSITION.BACK_RIGHT:
                visibleRedPosition = Slicer.SLICER_POSITION.FRONT_LEFT;
                break;
            case Slicer.SLICER_POSITION.FRONT_LEFT:
                visibleRedPosition = Slicer.SLICER_POSITION.BACK_RIGHT;
                break;
            case Slicer.SLICER_POSITION.FRONT_RIGHT:
                visibleRedPosition = Slicer.SLICER_POSITION.BACK_LEFT;
                break;
        }

        foreach (Slicer slicer in redSlicers)
        {
            slicer.gameObject.SetActive(false);
            if (slicer.slicerPosition == visibleRedPosition) slicer.gameObject.SetActive(true);
        }
    }
}
