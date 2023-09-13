using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static Texture2D GenerateTextureFromSprite(Sprite passInSprite)
    {
        var face = new Texture2D((int)passInSprite.rect.width, (int)passInSprite.rect.height);
        var pixels = passInSprite.texture.GetPixels((int)passInSprite.textureRect.x,
                                        (int)passInSprite.textureRect.y,
                                        (int)passInSprite.textureRect.width,
                                        (int)passInSprite.textureRect.height);

        face.SetPixels(pixels);
        face.Apply();
        return face;

    }
}
