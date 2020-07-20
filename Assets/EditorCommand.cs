using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCommand : MonoBehaviour
{
    public LayerMask ignoreMask;
    public PuzzleEditor puzzleEditor;

    Puzzle puzzle;
    COMMAND_MODE commandMode;

    private void Awake()
    {
        puzzle = puzzleEditor.GetPuzzle();
        commandMode = COMMAND_MODE.BUILD;
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

    enum COMMAND_MODE
    {
        DEFAULT,
        BUILD,
        DESTROY
    };

    private void Update()
    {
        EditorCube clickedCube;
        CLICKED_FACE clickedFace;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (ClickedCube(out clickedCube, out clickedFace))
            {
                int[] clickedCubeIndex = clickedCube.cubeIndex;
                int[] newCubeIndex = clickedCubeIndex;

                Debug.Log("CLicked Face : " + clickedFace);

                // 클릭 된 면에 붙어서 새로운 큐브가 생성
                switch(clickedFace)
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

                // 새로운 큐브가 9x9x9 범위를 벗어나지 않는지 확인
                if (newCubeIndex[0] < 9 && newCubeIndex[1] < 9 && newCubeIndex[2] < 9)
                {
                    float offSetX = 9 * .5f - .5f;
                    float offSetY = -9 * .5f + .5f;
                    float offSetZ = 9 * .5f - .5f;

                    Vector3 cubePosition = new Vector3(-newCubeIndex[2], 9 - newCubeIndex[1] - 1, -newCubeIndex[0]);
                    cubePosition += new Vector3(offSetX, offSetY, offSetZ);

                    var cubeObject = Instantiate(puzzleEditor.CubePrefab, cubePosition, Quaternion.identity, puzzleEditor.puzzleObject.transform);
                    cubeObject.GetComponent<EditorCube>().cubeIndex = newCubeIndex;
                }
            }
        }
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

            switch(hit.collider.gameObject.name)
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
