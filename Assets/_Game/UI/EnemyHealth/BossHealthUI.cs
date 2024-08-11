using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Image fillImage;

    public void SetFill(float factor)
    {
        fillImage.fillAmount = factor;
    }
}
