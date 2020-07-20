using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeCommand
{
    public Cube cube; // 클릭된 큐브
    protected Vector3 clickedPosition;    // 클릭된 스크린 좌표

    public virtual void Execute() { }
    public virtual void Undo() { }
}
