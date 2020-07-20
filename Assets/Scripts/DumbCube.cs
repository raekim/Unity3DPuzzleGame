using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbCube : MonoBehaviour
{
    Rigidbody rb;
    float rand = 7f;
    float forceRnd = 2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void SetRandomScale()
    {
        transform.localScale = Vector3.one * Random.Range(.26f, .3f);
    }

    public void RandomFall()
    {
        // 리셋
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        transform.position = transform.parent.position;

        SetRandomScale();
        rb.AddTorque(new Vector3(Random.Range(-rand, rand), Random.Range(-rand, rand), Random.Range(-rand, rand)), ForceMode.Impulse);
        rb.AddForce(new Vector3(Random.Range(-forceRnd, forceRnd), 5f, Random.Range(-forceRnd, forceRnd)), ForceMode.Impulse);
    }
}
