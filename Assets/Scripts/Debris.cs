using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    DumbCube[] dumbCubes;

    private void Awake()
    {
        dumbCubes = GetComponentsInChildren<DumbCube>();
    }

    private void Start()
    {
        PlayDebris();
    }

    void PlayDebris()
    {
        foreach (DumbCube cube in dumbCubes)
        {
            cube.RandomFall();
        }
    }
}
