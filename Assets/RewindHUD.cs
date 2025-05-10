using UnityEngine;
using UnityEngine.UI;

public class RewindHUD : MonoBehaviour
{
    public Image recordedCircle;
    public Image playbackCircle;
    public Image icon;

    public void UpdateRecordEffect(float percentage)
    {
        recordedCircle.fillAmount = percentage;
    }

    public void UpdatePlaybackEffect(float percentage)
    {
        playbackCircle.fillAmount = percentage;
    }

    public void ShowPlaybackCircle(bool show)
    {
        playbackCircle.gameObject.SetActive(show);
    }
}
