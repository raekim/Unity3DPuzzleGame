using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissLabel : MonoBehaviour
{
    Vector3 initialPosition;
    public GameObject MissLabelObject;

    private void Start()
    {
        initialPosition = MissLabelObject.transform.localPosition;
    }

    IEnumerator Flicker()
    {
        int count = 0;
        while (count++ < 8)
        {
            MissLabelObject.SetActive(!MissLabelObject.activeSelf);

            yield return new WaitForSeconds(.06f);
        }
    }


    IEnumerator FloatUp()
    {
        int count = 0;
        while(count++ < 7)
        {
            var missPosition = MissLabelObject.transform.localPosition;
            missPosition.y += 5f;
            MissLabelObject.transform.localPosition = missPosition;

            yield return new WaitForSeconds(.15f);
        }
    }

    public IEnumerator EffectCoroutine(Cube cube, Vector3 _initialPosition)
    {
        // 이펙트 재생
        MissLabelObject.transform.localPosition = _initialPosition;

        Debug.Log(MissLabelObject.transform.localPosition);
        MissLabelObject.SetActive(true);

        yield return FloatUp();

        yield return new WaitForSeconds(1f);

        yield return Flicker();

        MissLabelObject.SetActive(false);

        // 금 간 큐브 생성
    }
}
