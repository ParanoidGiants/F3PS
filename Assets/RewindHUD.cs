using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RewindHUD : MonoBehaviour
{
    public Image stateIcon;
    public Image playbackBar;

    public Sprite recording;
    public Sprite playing;
    public Sprite rewinding;
    public Sprite pausing;
    public Sprite none;

    public void UpdateRecordEffect(float percentage)
    {
        playbackBar.fillAmount = percentage;
    }

    public void UpdatePlaybackEffect(float percentage)
    {
        playbackBar.fillAmount = percentage;
    }

    public void ShowPlaybackBar(bool show)
    {
        playbackBar.transform.parent.gameObject.SetActive(show);
    }

    public void SetRecording()
    {
        stateIcon.sprite = recording;
    }

    public void SetPlaying()
    {
        stateIcon.sprite = playing;
    }

    public void SetNone()
    {
        stateIcon.sprite = null;
    }

    public void SetRewinding()
    {
        stateIcon.sprite = rewinding;
    }

    public void SetPausing()
    {
        stateIcon.sprite = pausing;
    }
}
