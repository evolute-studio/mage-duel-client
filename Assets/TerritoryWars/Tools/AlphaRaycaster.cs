using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.Tools
{
public class AlphaRaycaster : MonoBehaviour, ICanvasRaycastFilter
{
    public Image image;
    public float alphaThreshold = 0.1f;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (image == null || image.sprite == null) return false;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(image.rectTransform, sp, eventCamera, out local);

        Rect rect = image.GetPixelAdjustedRect();
        Vector2 normalized = new Vector2(
            (local.x - rect.x) / rect.width,
            (local.y - rect.y) / rect.height
        );

        Sprite sprite = image.sprite;
        Texture2D tex = sprite.texture;

        int x = Mathf.FloorToInt(normalized.x * sprite.texture.width);
        int y = Mathf.FloorToInt(normalized.y * sprite.texture.height);

        // межі
        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height)
            return false;

        Color pixel = tex.GetPixel(x, y);
        return pixel.a >= alphaThreshold;
    }
}
}
