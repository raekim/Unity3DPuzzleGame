using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeProtectCommand : cubeCommand
{
    Color protecColor;    // 보호 대상 조각을 색칠 할 색상
    private bool tryToProtect;    // true : 색칠을 하는 커맨드. false : 색칠을 없애는 커맨드

    public override void Execute()
    {
        if (tryToProtect)
        {
            if(!cube.isProtected)
            {
                // 조각을 보호색으로 색칠
                cube.SetColor(protecColor, (int)Cube.MATERIAL_INDEX.BACKGROUND);
                cube.isProtected = true;
                cube.PlayProtectAnimation();
            } 
        }
        else
        {
            if (cube.isProtected)
            {
                // 조각 색칠 해제
                cube.SetColor(Color.white, (int)Cube.MATERIAL_INDEX.BACKGROUND);
                cube.isProtected = false;
                cube.PlayProtectAnimation();
            }
        }
    }

    public override void Undo()
    {
        // Execute와 반대로 실행
        if (!tryToProtect)
        {
            if (!cube.isProtected)
            {
                // 조각을 보호색으로 색칠
                cube.SetColor(protecColor, (int)Cube.MATERIAL_INDEX.BACKGROUND);
                cube.isProtected = true;
            }
        }
        else
        {
            if (cube.isProtected)
            {
                // 조각 색칠 해제
                cube.SetColor(Color.white, (int)Cube.MATERIAL_INDEX.BACKGROUND);
                cube.isProtected = false;
            }
        }
    }

    public cubeProtectCommand(Cube _cube, Color _color, bool _tryToProtect)
    {
        cube = _cube;
        protecColor = _color;
        tryToProtect = _tryToProtect;
    }
}
