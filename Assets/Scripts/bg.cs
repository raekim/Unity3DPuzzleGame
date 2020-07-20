using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class bg : MonoBehaviour
{
    public float dist = 10f;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * dist;
        transform.LookAt(Camera.main.transform);
    }
}
