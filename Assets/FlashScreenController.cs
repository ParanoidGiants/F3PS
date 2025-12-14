using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FlashScreenController : MonoBehaviour
{
    private Image hudOverlay;

    private void Awake() { hudOverlay = GetComponent<Image>(); }

    public void CoverScreen(float duration)
    {
        hudOverlay.DOFade(1, duration);
    }

    public void UncoverScreen(float duration)
    {
        hudOverlay.DOFade(0, duration);
    }
}
