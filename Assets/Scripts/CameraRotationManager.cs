using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationManager : MonoBehaviour
{
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
        slicers.UpdateBlueSlicersVisibility(cameraLocation);
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
            slicers.UpdateBlueSlicersVisibility(cameraLocation);
        }

        Debug.Log(cameraLocation);
    }

    //public IEnumerator RotateAroundCompletePuzzle(float time)
    //{
    //    float t = 0f;
    //    float yRotTime = 2.5f;
    //    float yAmountDelta = yAmount / yRotTime;
    //    float yAmountCnt = 0f;
    //
    //    float xRotSpeed = speed * 2f;
    //    float yRotSpeed = 1f;
    //
    //    float xSpeedMult = 0f;
    //
    //    // time 동안 빙글빙글 돌면서 세로 회전 맞추기
    //    while (t < time)
    //    {
    //        // 세로 회전 맞춤
    //        if(yAmountCnt <= yRotTime)
    //        {
    //            transform.RotateAround(puzzleObj.position, -transform.right, -yAmountDelta * yRotSpeed * Time.deltaTime);
    //        }
    //
    //        // 가로 회전 빙글빙글~~~
    //        transform.RotateAround(puzzleObj.position, transform.up, -1f * xRotSpeed * xSpeedMult * Time.deltaTime);
    //
    //        transform.LookAt(puzzleObj);
    //
    //        yAmountCnt += Time.deltaTime;
    //        t += Time.deltaTime;
    //
    //        xSpeedMult += Time.deltaTime / 2f;
    //        xSpeedMult = Mathf.Min(xSpeedMult, 1f);
    //
    //        yield return null;
    //    }
    //
    //    // 세로 회전 맞추기 끝
    //    yAmount = 0f;
    //
    //    // 2바퀴 마저 돌면서 속도 점점 낮추기
    //    int rotationLaps = -1;
    //    bool insideDefaultRange = false;
    //    bool insideSlowDownRange = false;
    //    bool startSlowingDown = false;
    //    float slowDownRange = 35f;
    //
    //    while(rotationLaps < 2)
    //    {
    //        // 가로 회전 빙글빙글~~~
    //        transform.RotateAround(puzzleObj.position, transform.up, -1f * xRotSpeed * xSpeedMult * Time.deltaTime);
    //        transform.LookAt(puzzleObj);
    //
    //        if(startSlowingDown)
    //        {
    //            xRotSpeed -= xRotSpeed * (Time.deltaTime / 2f);
    //            xRotSpeed = Mathf.Max(xRotSpeed, 3f);
    //        }
    //
    //        if (!insideSlowDownRange && Vector3.Distance(camDefaultTrans, transform.position) < slowDownRange)
    //        {
    //            insideSlowDownRange = true;
    //        }
    //        else if (insideSlowDownRange && Vector3.Distance(camDefaultTrans, transform.position) > slowDownRange)
    //        {
    //            insideSlowDownRange = false;
    //            if (rotationLaps == 1) startSlowingDown = true;
    //        }
    //
    //        if (!insideDefaultRange && Vector3.Distance(camDefaultTrans, transform.position) < 5f)
    //        {
    //            insideDefaultRange = true;
    //            rotationLaps++;
    //        }
    //        else if (insideDefaultRange && Vector3.Distance(camDefaultTrans, transform.position) > 5f)
    //        {
    //            insideDefaultRange = false;
    //        }
    //
    //        yield return null;
    //    }
    //}
}
