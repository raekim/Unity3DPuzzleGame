using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCommand : MonoBehaviour
{
    enum COMMAND_MODE
    {
        DEFAULT,
        BUILD,
        DESTROY,
        COLOR
    }

    enum CLICKED_FACE
    {
        FRONT,
        BACK,
        LEFT,
        RIGHT,
        TOP,
        BOTTOM
    }

    public LayerMask ignoreMask;
    public PuzzleEditor puzzleEditor;
    public camRot cameraRot;

    COMMAND_MODE commandMode;

    KeyCode buildKey = KeyCode.Alpha1;
    KeyCode destroyKey = KeyCode.Alpha2;
    KeyCode colorKey = KeyCode.Alpha3;

    private void Awake()
    {
        commandMode = COMMAND_MODE.DEFAULT;
        Debug.Log("1:BUILD, 2:DESTROY, 3:COLOR");
    }

    private void Update()
    {
        EditorCube clickedCube;
        CLICKED_FACE clickedFace;

        ChangeCommandMode();

        switch (commandMode)
        {
            case COMMAND_MODE.DEFAULT:
                break;
            case COMMAND_MODE.BUILD:
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (ClickedCube(out clickedCube, out clickedFace))
                    {
                        // [z,y,x]
                        int[] newCubeIndex = new int[3];
                        for (int i = 0; i < 3; ++i) newCubeIndex[i] = clickedCube.cubeIndex[i];

                        // 클릭 된 면에 붙어서 새로운 큐브가 생성
                        switch (clickedFace)
                        {
                            case CLICKED_FACE.FRONT:
                                newCubeIndex[0]--;
                                break;
                            case CLICKED_FACE.BACK:
                                newCubeIndex[0]++;
                                break;
                            case CLICKED_FACE.BOTTOM:
                                newCubeIndex[1]++;
                                break;
                            case CLICKED_FACE.TOP:
                                newCubeIndex[1]--;
                                break;
                            case CLICKED_FACE.LEFT:
                                newCubeIndex[2]++;
                                break;
                            case CLICKED_FACE.RIGHT:
                                newCubeIndex[2]--;
                                break;

                        }

                        puzzleEditor.AddNewCube(newCubeIndex);
                    }
                }
                break;
            case COMMAND_MODE.DESTROY:
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (ClickedCube(out clickedCube, out clickedFace))
                    {
                        puzzleEditor.DestoryCube(clickedCube.cubeIndex);
                    }
                }
                break;
        }
    }

    void ChangeCommandMode()
    {
        if (!Input.anyKey)
        {
            SetCommandMode(COMMAND_MODE.DEFAULT);
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            SetCommandMode(COMMAND_MODE.DESTROY);
        }
        else if (Input.GetKeyDown(buildKey))
            {
                SetCommandMode(COMMAND_MODE.BUILD);
            }
    }

    void SetCommandMode(COMMAND_MODE mode)
    {
        commandMode = mode;

        cameraRot.camRotAllowed = (mode == COMMAND_MODE.DEFAULT);
    }

    // 큐브 조각을 클릭했나?
    bool ClickedCube(out EditorCube _clickedCube, out CLICKED_FACE _clickedFace)
    {
        _clickedFace = CLICKED_FACE.FRONT;
        _clickedCube = null;

        if (!Input.GetKey(KeyCode.Mouse0)) return false;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, float.MaxValue, ~ignoreMask))
        {
            _clickedCube = hit.collider.GetComponentInParent<EditorCube>();
            switch (hit.collider.gameObject.name)
            {
                case "front":
                    _clickedFace = CLICKED_FACE.FRONT;
                    break;
                case "back":
                    _clickedFace = CLICKED_FACE.BACK;
                    break;
                case "left":
                    _clickedFace = CLICKED_FACE.LEFT;
                    break;
                case "right":
                    _clickedFace = CLICKED_FACE.RIGHT;
                    break;
                case "top":
                    _clickedFace = CLICKED_FACE.TOP;
                    break;
                case "bottom":
                    _clickedFace = CLICKED_FACE.BOTTOM;
                    break;
            }
        }

        return _clickedCube != null;
    }
}
