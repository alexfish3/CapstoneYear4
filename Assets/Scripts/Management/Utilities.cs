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

    public static bool SetColor(ref Color currentValue, Color newValue)
    {
        if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            return false;

        currentValue = newValue;
        return true;
    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return false;

        currentValue = newValue;
        return true;
    }
}
