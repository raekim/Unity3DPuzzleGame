using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationManager : MonoBehaviour
{
    // 퍼즐의 어떤 부분을 바라보고 있느냐에 따라 나뉘는 카메라 위치
    public enum CAMERA_LOCATION
    {
        FRONT_LEFT,
        FRONT_RIGHT,
        BACK_LEFT,
        BACK_RIGHT
    }

    public Transform puzzleObj;
    public Slicers slicers;
    [HideInInspector] public bool camRotAllowed;

    readonly float rotateSpeed = 70f;
    bool mouseClicked;
    float yAmount = 0f; // 누적 세로 회전 정보
    CAMERA_LOCATION cameraLocation;

    private void Awake()
    {
        camRotAllowed = true;
        cameraLocation = CAMERA_LOCATION.FRONT_RIGHT;
    }

    private void Start()
    {
        if(slicers) slicers.UpdateBlueSlicersVisibility(cameraLocation);
    }

    // Update is called once per frame
    void Update()
    {
        if (!camRotAllowed) return;

        mouseClicked = Input.GetKey(KeyCode.Mouse0);

        if (mouseClicked)
        {
            float mouseY = Input.GetAxis("Mouse Y");
            float mouseX = Input.GetAxis("Mouse X");

            // 퍼즐 거꾸로 뒤집기 방지
            var YDot = Vector3.Dot(puzzleObj.up, (transform.position - puzzleObj.position).normalized);

            if (Mathf.Abs(YDot) < 0.9f || Mathf.Sign(mouseY) == Mathf.Sign(YDot))
            {
                transform.RotateAround(puzzleObj.position, -transform.right, mouseY * rotateSpeed * Time.deltaTime);
                //Debug.Log("YDot: " + YDot);
            }

            transform.RotateAround(puzzleObj.position, transform.up, mouseX * rotateSpeed * Time.deltaTime);

            transform.LookAt(puzzleObj);

            DetermineCameraLocation();
        }
    }

    void DetermineCameraLocation()
    {
        var camPos = transform.position;
        camPos.y = puzzleObj.position.y;
        var angle = Vector3.SignedAngle(puzzleObj.forward, (camPos - puzzleObj.position).normalized, puzzleObj.up);

        int angleInt = Mathf.RoundToInt(angle);

        CAMERA_LOCATION newCameraLocation = cameraLocation;

        if (-90 <= angleInt && angleInt <= 0)
        {
            newCameraLocation = CAMERA_LOCATION.FRONT_RIGHT;
        }
        else if (0 <= angleInt && angleInt <= 90)
        {
            newCameraLocation = CAMERA_LOCATION.FRONT_LEFT;
        }
        else if (90 <= angleInt && angleInt <= 180)
        {
            newCameraLocation = CAMERA_LOCATION.BACK_LEFT;
        }
        else if (-180 <= angleInt && angleInt <= -90)
        {
            newCameraLocation = CAMERA_LOCATION.BACK_RIGHT;
        }

        if(cameraLocation != newCameraLocation)
        {
            cameraLocation = newCameraLocation;
            if(slicers) slicers.UpdateBlueSlicersVisibility(cameraLocation);
        }

        Debug.Log(cameraLocation);
    }
}
