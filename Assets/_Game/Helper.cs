using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class Helper
{
    public static IEnumerator UpdateLayoutGroups(RectTransform rectTransform)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}