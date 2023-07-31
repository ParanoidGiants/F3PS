using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class Helper
{
    public static LayerMask PlayerLayer => LayerMask.GetMask("Character");
    public static LayerMask DefaultLayer => LayerMask.GetMask("Default");
    public static IEnumerator UpdateLayoutGroups(RectTransform rectTransform)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public static bool IsLayerPlayerLayer(int layer)
    {
        var colliderLayer = 1 << layer;
        var result = colliderLayer & PlayerLayer;
        return result != 0;
    }

    public static bool IsLayerDefaultLayer(int layer)
    {
        var colliderLayer = 1 << layer;
        var result = colliderLayer & DefaultLayer;
        return result != 0;
    }
}