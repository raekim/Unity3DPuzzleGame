using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public MissLabel missLabel;
    public CommandManager commandManager;

    public static EffectsManager Instance;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void CubeCrackEffect(Cube cube, Vector3 spawnPosition)
    {
        StartCoroutine(CubeCrackEffectCoroutine(cube, spawnPosition));
    }

    public IEnumerator CubeCrackEffectCoroutine(Cube cube, Vector3 spawnPosition)
    {
        commandManager.commandFreeze = true;

        // MISS! 이펙트 재생
        yield return StartCoroutine(missLabel.EffectCoroutine(cube, spawnPosition));

        // 잘못 부순 큐브가 금 간 상태로 뜨기
        cube.GetCracked();
        cube.gameObject.SetActive(true);

        commandManager.commandFreeze = false;
    }
}
