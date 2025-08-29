using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FlashScreenController : MonoBehaviour
{
    private Image hudOverlay;

    private void Awake() { hudOverlay = GetComponent<Image>(); }

    public void CoverScreen()
    {
        hudOverlay.color = new Color(1, 1, 1, 1);
    }

    public void UncoverScreen()
    {
        hudOverlay.DOFade(0, 0.25f);
    }
}
