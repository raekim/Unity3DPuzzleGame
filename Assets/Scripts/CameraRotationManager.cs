using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationManager : MonoBehaviour
{
    public Transform puzzleObj;
    public float speed = 1f;
    bool clicked = false;


    float yAmount = 0f; // 누적 세로 회전 정보
    public Vector3 camDefaultTrans;
    Vector3 defaultForwardVector;

    public bool camRotAllowed = true;

    private void Awake()
    {
        transform.LookAt(puzzleObj);
        defaultForwardVector = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (!camRotAllowed) return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) clicked = true;
        if (Input.GetKeyUp(KeyCode.Mouse0)) clicked = false;

        if (clicked)
        {
            float mouseY = Input.GetAxis("Mouse Y");
            float mouseX = Input.GetAxis("Mouse X");

            var ag = Vector3.Dot(puzzleObj.up, (transform.position - puzzleObj.position).normalized);

            // 퍼즐 거꾸로 뒤집기 방지
            if (Mathf.Abs(ag) >= 0.95f)
            {
                if (Mathf.Sign(mouseY) == Mathf.Sign(ag))
                {
                    transform.RotateAround(puzzleObj.position, -transform.right, mouseY * speed * Time.deltaTime);
                    yAmount += mouseY;
                }
            }
            else
            {
                transform.RotateAround(puzzleObj.position, -transform.right, mouseY * speed * Time.deltaTime);
                yAmount += mouseY;
            }

            transform.RotateAround(puzzleObj.position, transform.up, mouseX * speed * Time.deltaTime);

            transform.LookAt(puzzleObj);
        }

        // Debug.Log(yAmount);
    }

    public IEnumerator RotateAroundCompletePuzzle(float time)
    {
        float t = 0f;
        float yRotTime = 2.5f;
        float yAmountDelta = yAmount / yRotTime;
        float yAmountCnt = 0f;

        float xRotSpeed = speed * 2f;
        float yRotSpeed = 1f;

        float xSpeedMult = 0f;

        // time 동안 빙글빙글 돌면서 세로 회전 맞추기
        while (t < time)
        {
            // 세로 회전 맞춤
            if(yAmountCnt <= yRotTime)
            {
                transform.RotateAround(puzzleObj.position, -transform.right, -yAmountDelta * yRotSpeed * Time.deltaTime);
            }

            // 가로 회전 빙글빙글~~~
            transform.RotateAround(puzzleObj.position, transform.up, -1f * xRotSpeed * xSpeedMult * Time.deltaTime);

            transform.LookAt(puzzleObj);

            yAmountCnt += Time.deltaTime;
            t += Time.deltaTime;

            xSpeedMult += Time.deltaTime / 2f;
            xSpeedMult = Mathf.Min(xSpeedMult, 1f);

            yield return null;
        }

        // 세로 회전 맞추기 끝
        yAmount = 0f;

        // 2바퀴 마저 돌면서 속도 점점 낮추기
        int rotationLaps = -1;
        bool insideDefaultRange = false;
        bool insideSlowDownRange = false;
        bool startSlowingDown = false;
        float slowDownRange = 35f;

        while(rotationLaps < 2)
        {
            // 가로 회전 빙글빙글~~~
            transform.RotateAround(puzzleObj.position, transform.up, -1f * xRotSpeed * xSpeedMult * Time.deltaTime);
            transform.LookAt(puzzleObj);

            if(startSlowingDown)
            {
                xRotSpeed -= xRotSpeed * (Time.deltaTime / 2f);
                xRotSpeed = Mathf.Max(xRotSpeed, 3f);
            }

            if (!insideSlowDownRange && Vector3.Distance(camDefaultTrans, transform.position) < slowDownRange)
            {
                insideSlowDownRange = true;
            }
            else if (insideSlowDownRange && Vector3.Distance(camDefaultTrans, transform.position) > slowDownRange)
            {
                insideSlowDownRange = false;
                if (rotationLaps == 1) startSlowingDown = true;
            }

            if (!insideDefaultRange && Vector3.Distance(camDefaultTrans, transform.position) < 5f)
            {
                insideDefaultRange = true;
                rotationLaps++;
            }
            else if (insideDefaultRange && Vector3.Distance(camDefaultTrans, transform.position) > 5f)
            {
                insideDefaultRange = false;
            }

            yield return null;
        }
    }
}
