using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerCapsule : MonoBehaviour
{
    public Color inactiveColor;
    public Color activeColor;
    public CameraRotationManager cameraRot;

    public delegate void OnMouseDownDelegate();
    public event OnMouseDownDelegate capsuleClickDelegate;

    public delegate void OnMouseDragDelegate();
    public event OnMouseDragDelegate capsuleDragDelegate;

    Material myMaterial;

    private void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>().material;
        inactiveColor.a = activeColor.a = 1f;
    }

    private void OnMouseDown()
    {
        cameraRot.camRotAllowed = false;
        capsuleClickDelegate();

        // 클릭하면 색상 변경
        myMaterial.color = activeColor;
    }

    private void OnMouseUp()
    {
        cameraRot.camRotAllowed = true;

        myMaterial.color = inactiveColor;
    }

    private void OnMouseDrag()
    {
        capsuleDragDelegate();
    }
}
