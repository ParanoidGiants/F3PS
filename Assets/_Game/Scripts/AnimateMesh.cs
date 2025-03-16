using DG.Tweening;
using System;
using UnityEngine;

public class AnimateMesh : MonoBehaviour
{
    private Renderer _renderer;

    private bool setEmission = false;
    private Color CurrentEmissionColor => currentTimeEmissionColor + currentHitFlashEmissionColor;

    [Space(10)]
    [Header("Debug")]
    public bool hitFlashAnimate = false;
    public bool timeFlashAnimate = false;
    public float timeFlashAnimateTimeScale = 1f;
    
    public float timeScale = 1f;
    public void SetTimeScale(float timeScale_)
    {
        DOTween.To(
            () => this.timeScale,
            x =>
            {
                this.timeScale = x;
                if (_timeSequence != null && _timeSequence.IsActive() && _timeSequence.IsPlaying())
                {
                    _timeSequence.timeScale = this.timeScale;
                }
                if (_hitFlashSequence != null && _hitFlashSequence.IsActive() && _hitFlashSequence.IsPlaying())
                {
                    _hitFlashSequence.timeScale = this.timeScale;
                }
            },
            timeScale_,
            1f
        ).SetEase(Ease.Linear);
    }


    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        foreach (var material in _renderer.materials)
        {
            material.EnableKeyword(GlobalConstants.MATERIAL_KEYWORD_EMISSION);
        }
    }

    public void Update()
    {
        if (setEmission)
        {
            setEmission = false;
            SetEmission(CurrentEmissionColor);
        }

        if (hitFlashAnimate)
        {
            hitFlashAnimate = false;
            HitFlash();
        }

        if (timeFlashAnimate)
        {
            timeFlashAnimate = false;
            TimeFlash(timeFlashAnimateTimeScale);
        }
    }

    private void OnDestroy()
    {
        if (_hitFlashSequence != null)
        {
            _hitFlashSequence.Kill();
            _hitFlashSequence = null;
        }
        if (_timeSequence != null)
        {
            _timeSequence.Kill();
            _timeSequence = null;
        }
    }

    private void SetEmission(Color color)
    {
        foreach (var material in _renderer.materials)
        {
            material.SetColor(GlobalConstants.MATERIAL_KEYWORD_EMISSION_COLOR, color);
        }
    }

    [Header("Time Animation Settings")]
    [Range(0f, 1f)]
    public float timeAnimationDuration = 0.1f;
    public Color timeEmissionColor;
    public Color currentTimeEmissionColor = Color.black;
    private Sequence _timeSequence;
    public void TimeFlash(float newTimeScale)
    {
        if (_timeSequence != null && _timeSequence.IsActive() && _timeSequence.IsPlaying())
        {
            _timeSequence.Kill();
        }
        _timeSequence = DOTween.Sequence();
        if (newTimeScale != 1f)
        {
            _timeSequence.Insert(
                0f,
                DOTween.To(
                    () => currentTimeEmissionColor,
                    x => {
                        currentTimeEmissionColor = x;
                        setEmission = true;
                    },
                    timeEmissionColor,
                    timeAnimationDuration
                )
                .SetEase(Ease.InSine)
            );
        }
        else
        {
            _timeSequence.Insert(
                0f,
                DOTween.To(
                    () => currentTimeEmissionColor,
                    x => {
                        currentTimeEmissionColor = x;
                        setEmission = true;
                    },
                    Color.black,
                    timeAnimationDuration
                )
                .SetEase(Ease.OutSine)
            );
        }
        _timeSequence.timeScale = timeScale;
        _timeSequence.Play();
    }


    [Space(20)]
    [Header("Hit Flash Animation Settings")]
    public Color hitFlashEmissionColor;
    public Color currentHitFlashEmissionColor = Color.black;
    public float hitFlashEaseInAnimationDuration;
    public float hitFlashEaseOutAnimationDuration;
    public Ease hitFlashEaseIn;
    public Ease hitFlashEaseOut;
    private Sequence _hitFlashSequence;

    public void HitFlash()
    {
        if (_hitFlashSequence != null && _hitFlashSequence.IsActive() && _hitFlashSequence.IsPlaying())
        {
            _hitFlashSequence.Kill();
        }
        _hitFlashSequence = DOTween.Sequence();
        _hitFlashSequence.Insert(
            0f,
            DOTween.To(
                () => currentHitFlashEmissionColor,
                x => {
                    currentHitFlashEmissionColor = x;
                    setEmission = true;
                },
                hitFlashEmissionColor,
                hitFlashEaseInAnimationDuration
            )
            .SetEase(hitFlashEaseIn)
        );
        _hitFlashSequence.Insert(
            hitFlashEaseInAnimationDuration,
            DOTween.To(
                () => currentHitFlashEmissionColor,
                x => {
                    currentHitFlashEmissionColor = x;
                    setEmission = true;
                },
                Color.black,
                hitFlashEaseInAnimationDuration
            )
            .SetEase(hitFlashEaseOut)
        );
        _hitFlashSequence.timeScale = timeScale;
        _hitFlashSequence.Play();
    }

    public void FadeOut(float fadeDuration)
    {
        var currentAlpha = 1f;
        Tweener dying = DOTween.To(
            () => currentAlpha,
            x => {
                currentAlpha = x;
                foreach (var material in _renderer.materials)
                {
                    material.SetFloat("_Alpha", currentAlpha);
                }
            },
            0f,
            fadeDuration
        );
        dying.timeScale = timeScale;
        dying.Play();
    }
}
