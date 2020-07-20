using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    public LayerMask ignoreMask;
    public camRot cameraRot;
    public bool commandFreeze = false;

    KeyCode destroyKey = KeyCode.Alpha1;
    KeyCode protectKey = KeyCode.Alpha2;

    Cube clickedCube;

    enum commandMode
    {
        DEFAULT,
        DESTROY,
        PROTECT
    };

    private bool tryToProtect;

    commandMode myCommandState;

    List<cubeCommand> lastCubeCommands;
    int lastCommandIdx = -1;

    public void SetCommandStateToDestroy()
    {
        myCommandState = commandMode.DESTROY;
        cameraRot.camRotAllowed = false;
    }

    public void SetCommandStateToDefault()
    {
        myCommandState = commandMode.DEFAULT;
        cameraRot.camRotAllowed = true;
        
        // 현재 조작 모드가 destroy 또는 protect라면 적절한 UI애니메이션 재생
    }

    public void SetCommandStateToProtect()
    {
        myCommandState = commandMode.PROTECT;
        cameraRot.camRotAllowed = false;
    }

    private void Awake()
    {
        lastCubeCommands = new List<cubeCommand>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        SetCommandStateToDefault();
    }

    void ChangeCommandMode()
    {
        // 아무 키도 누르고 있지 않은 경우 rotate 모드로 돌아온다
        if(!Input.anyKey)
        {
            SetCommandStateToDefault();
        }

        // 조작 모드 변경 : rotate, destroy, protect 3가지 모드
        switch (myCommandState)
        {
            case commandMode.DEFAULT:
                if (Input.GetKey(destroyKey))
                {
                    SetCommandStateToDestroy();
                }
                if (Input.GetKey(protectKey))
                {
                    SetCommandStateToProtect();
                }
                break;
            case commandMode.DESTROY:
                if (Input.GetKeyUp(destroyKey))
                {
                    SetCommandStateToDefault();
                }
                break;
            case commandMode.PROTECT:
                if (Input.GetKeyUp(protectKey))
                {
                    SetCommandStateToDefault();
                }
                break;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (commandFreeze) return;

        ChangeCommandMode();

        // 큐브 파괴
        if (myCommandState == commandMode.DESTROY)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && ClickedCube(out clickedCube))
            {
                var clickedPosition = Input.mousePosition;
                clickedPosition.z = 0f;
                clickedPosition.x -= Screen.width * .5f;
                clickedPosition.y -= Screen.height * .5f;;

                cubeCommand destroyCommand = new cubeDestoryCommand(clickedCube.GetComponent<Cube>(), clickedPosition);

                lastCubeCommands.Add(destroyCommand);
                lastCommandIdx++;

                destroyCommand.Execute();

                // 성공적인 큐브 파괴를 퍼즐매니저에 전달
                if(!destroyCommand.cube.isAnswerCube)
                    puzzleManager.RegisterCubeDestroy();
            }
        }
        // 큐브 색칠(보호)
        else if (myCommandState == commandMode.PROTECT)
        {
            if(ClickedCube(out clickedCube))
            {
                cubeCommand protectCommand;

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    tryToProtect = !(clickedCube.GetComponent<Cube>().isProtected);
                }

                // 드래그 해서 연속 색칠
                protectCommand = new cubeProtectCommand(clickedCube.GetComponent<Cube>(), Color.cyan, tryToProtect);

                lastCubeCommands.Add(protectCommand);
                lastCommandIdx++;

                protectCommand.Execute();
            }
        }
    }

    // 큐브 조각을 클릭했나?
    private bool ClickedCube(out Cube _clickedCube)
    {
        _clickedCube = null;

        if (!Input.GetKey(KeyCode.Mouse0)) return false;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, float.MaxValue, ~ignoreMask))
        {
            _clickedCube = hit.collider.gameObject.GetComponent<Cube>();
        }

        return _clickedCube != null;
    }
}
