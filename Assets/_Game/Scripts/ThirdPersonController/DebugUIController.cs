using UnityEngine;

public class DebugUIController : MonoBehaviour
{
    [Header("References")]
    public GameObject freeCameraText;
    public GameObject pausedText;
    public GameObject slowMoText;

    public void ShowPauseText()
    {
        pausedText.SetActive(true);
    }
    public void HidePauseText()
    {
        pausedText.SetActive(false);
    }

    public void ShowSlowMoText()
    {
        slowMoText.SetActive(true);
    }

    public void HideSlowMoText()
    {
        slowMoText.SetActive(false);
    }

    public void ShowFreeCameraText()
    {
        freeCameraText.SetActive(true);
    }

    public void HideFreeCameraText()
    {
        freeCameraText.SetActive(false);
    }
}
