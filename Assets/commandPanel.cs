using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class commandPanel : MonoBehaviour
{
    public UISprite[] buttonSprites = new UISprite[3];

    private void Start()
    {
        ClickedButton(buttonSprites[1]);
    }

    public void ClickedButton(UISprite clikedButtonSprite)
    {
        for (int i = 0; i < 3; ++i)
        {
            buttonSprites[i].color = new Color(.3f, .3f, .3f, 1f);
        }

        clikedButtonSprite.color = Color.white;
    }
}
