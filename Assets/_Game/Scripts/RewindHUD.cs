using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RewindHUD : MonoBehaviour
{
    public GameObject rewindBar;
    public Image stateIcon;
    public Image playbackBar;

    private Sequence _animation;
    public Sprite recording;
    public Sprite playing;
    public Sprite rewinding;
    public Sprite pausing;

    private void Awake()
    {
        _animation = DOTween.Sequence()
            .Append(stateIcon.DOFade(0.2f, 0.5f))
            .Append(stateIcon.DOFade(1f, 0.5f))
            .SetLoops(-1);
    }

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
        stateIcon.enabled = true;
        if (!_animation.IsPlaying())
        {
            _animation.Restart();
        }
        stateIcon.sprite = recording;
    }

    public void SetPlaying()
    {
        stateIcon.sprite = playing;
    }

    public void SetNone()
    {
        _animation.Pause();
        stateIcon.enabled = false;
    }

    public void SetRewinding()
    {
        stateIcon.sprite = rewinding;
    }

    public void SetPausing()
    {
        _animation.Pause();
        stateIcon.color = Color.white;
        stateIcon.sprite = pausing;
    }

    private void OnDisable()
    {
        rewindBar.SetActive(false);
    }

    private void OnEnable()
    {
        rewindBar.SetActive(true);
    }
}
