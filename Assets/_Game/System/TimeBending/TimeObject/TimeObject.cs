using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime;

    [Space(10)]
    [Header("Animation References")]
    public Transform animateTransform;
    public List<Renderer> renderers;
    [Space(10)]
    [Header("Animation Settings")]
    [Range(0f, 1f)]
    public float scaleAnimationOffset = 0.2f;
    public float animationDuration = 0.1f;
    public Color emissionColor = new(0f, 191f/255f, 191f,255f);
    [Range(0f, 4f)]
    public float emissionEffect = 1f;
    private Vector3 _originalScale;
    private Sequence _sequence;

    private void Awake()
    {
        _originalScale = animateTransform.localScale;
    }

    void Start()
    {
        PitchTimeScale(currentTimeScale);
    }

    public virtual void PitchTimeScale(float newTimeScale)
    {
        if (currentTimeScale == newTimeScale)
        {
            return;
        }
        Flash(newTimeScale);
        currentTimeScale = newTimeScale;
    }

    private void Flash(float newTimeScale)
    {
        if (_sequence != null && _sequence.IsActive() && _sequence.IsPlaying())
        {
            _sequence.Kill();
            animateTransform.localScale = _originalScale;
        }
        _sequence = DOTween.Sequence();
        if (newTimeScale != 1f)
        {
            _sequence.Append(animateTransform.DOScale(_originalScale * (1f - scaleAnimationOffset), animationDuration).SetEase(Ease.OutSine));
            _sequence.Append(animateTransform.DOScale(_originalScale, animationDuration).SetEase(Ease.OutSine));
            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                _sequence.InsertCallback(0f, () => { material.EnableKeyword(GlobalConstants.MATERIAL_KEYWORD_EMISSION); });
                _sequence.Insert(0f, material.DOColor(emissionEffect * emissionColor, GlobalConstants.MATERIAL_KEYWORD_EMISSION_COLOR, animationDuration).SetEase(Ease.InSine));
            }
        }
        else
        {
            _sequence.Append(animateTransform.DOScale(_originalScale * (1f + scaleAnimationOffset), animationDuration).SetEase(Ease.OutSine));
            _sequence.Append(animateTransform.DOScale(_originalScale, animationDuration).SetEase(Ease.OutSine));
            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                _sequence.Insert(0f, material.DOColor(Color.black, GlobalConstants.MATERIAL_KEYWORD_EMISSION_COLOR, animationDuration).SetEase(Ease.OutSine));
                _sequence.InsertCallback(animationDuration, () => { material.DisableKeyword(GlobalConstants.MATERIAL_KEYWORD_EMISSION); });
            }
        }
        _sequence.Play();
    }

    protected virtual void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }

    private void OnDestroy()
    {
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
}
