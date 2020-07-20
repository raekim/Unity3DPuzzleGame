using UnityEngine;

public class cubeDestoryCommand : cubeCommand
{
    bool destroySuccess = false;

    public override void Execute() 
    {
        if (cube.isDestroyed) return;

        if (!cube.isProtected && !cube.isCracked)
        {
            // 큐브를 부수고 숨긴다
            cube.PlayDestoryEffect();

            if (cube.isAnswerCube)
            {
                EffectsManager.Instance.CubeCrackEffect(cube, clickedPosition);
            }
            else
            {
                // 조각 파괴 성공
                destroySuccess = true;
                cube.isDestroyed = true;
            }
        }
        else
        {
            // 색칠 되어 있거나 금이 간 조각은 파괴 불가능. 애니메이션 등 효과 재생
            cube.PlayshakeAnimation();
        }
    }

    public override void Undo()
    {
        if (destroySuccess)
        {
            // 정상적으로 조각이 파괴되었던 경우에만 복원시킴
            cube.gameObject.SetActive(true);
        }
    }

    public cubeDestoryCommand(Cube _cube, Vector3 _clickedPosition)
    {
        cube = _cube;
        clickedPosition = _clickedPosition;
    }
}
