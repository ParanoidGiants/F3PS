using DG.Tweening;
using F3PS;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AnubisScrollHUD : MonoBehaviour
{
    private AnubisScrollSkillData AnubisScrollData => GameManager.Instance.GameData.PlayerData.AnubisScrollSkillData;
    private PlayerEventController PlayerEventController => GameManager.Instance.saveGameManager.PlayerEventController;

    public GameObject anubisScrollBar;
    public Image stateIcon;
    public Image playbackBar;

    private Sequence _animation;
    public Sprite recording;
    public Sprite playback;
    public Sprite rewind;
    public Sprite paused;

    private void OnEnable()
    {
        PlayerEventController.OnAnubisScrollStateChanged += UpdateScrollState;
        PlayerEventController.OnAnubisScrollCurrentFrameChanged += UpdateCurrentFrame;
        PlayerEventController.OnAnubisScrollTotalFramesChanged += UpdateTotalFrames;
        anubisScrollBar.SetActive(true);
    }

    private void OnDisable()
    {
        PlayerEventController.OnAnubisScrollStateChanged -= UpdateScrollState;
        PlayerEventController.OnAnubisScrollCurrentFrameChanged -= UpdateCurrentFrame;
        PlayerEventController.OnAnubisScrollTotalFramesChanged -= UpdateTotalFrames;
        anubisScrollBar.SetActive(false);
    }

    private void Awake()
    {
        _animation = DOTween.Sequence()
            .Append(stateIcon.DOFade(0.2f, 0.5f))
            .Append(stateIcon.DOFade(1f, 0.5f))
            .SetLoops(-1);
    }

    private void UpdateCurrentFrame(int currentFrame)
    {
        var totalFrames = AnubisScrollData.TotalFrames;
        UpdatePlaybackEffect(currentFrame, totalFrames);
    }

    private void UpdateTotalFrames(int totalFrames)
    {
        var currentFrame = AnubisScrollData.CurrentFrame;
        UpdatePlaybackEffect(currentFrame, totalFrames);
    }

    private void UpdatePlaybackEffect(int currentFrame, int totalFrameCount)
    {
        if (totalFrameCount == 0f)
        {
            playbackBar.fillAmount = 1f;
        }
        else
        {
            playbackBar.fillAmount = (float)currentFrame / totalFrameCount;
        }
    }

    private void UpdateScrollState(AnubisScrollState state)
    {
        switch (state)
        {
            case AnubisScrollState.Record:
                stateIcon.enabled = true;
                if (!_animation.IsPlaying())
                {
                    _animation.Restart();
                }
                stateIcon.sprite = recording;
                playbackBar.gameObject.SetActive(true);
                break;
            case AnubisScrollState.Playback:
                stateIcon.sprite = playback;
                break;
            case AnubisScrollState.None:
                _animation.Pause();
                stateIcon.enabled = false;
                break;
            case AnubisScrollState.Rewind:
                stateIcon.sprite = rewind;
                break;
            case AnubisScrollState.Paused:
                _animation.Pause();
                playbackBar.gameObject.SetActive(true);
                stateIcon.color = Color.white;
                stateIcon.sprite = paused;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}
