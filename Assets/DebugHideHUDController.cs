using System;
using UnityEngine;

public class DebugHideHUDController : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    internal void HideHud() { canvasGroup.alpha = 0f; }

    internal void ShowHud() { canvasGroup.alpha = 1f; }
}
