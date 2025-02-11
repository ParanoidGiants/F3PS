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

    [Space(20)]
    [Header("Animation")]
    public Transform animateTransform;
    public List<Renderer> renderers;
    [Range(0f, 1f)]
    public float scaleAnimationOffset = 0.2f;
    public float animationDuration = 0.1f;
    private Vector3 _originalScale;
    private Sequence _sequence;
    private Color _emissionColor = new Color(0f, 191f/255f, 191f,255f);

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
        Debug.Log($"PITCH {gameObject.name} from {currentTimeScale} to {newTimeScale}");
        if (_sequence != null && _sequence.IsActive() && _sequence.IsPlaying())
        {
            _sequence.Kill();
            animateTransform.localScale = _originalScale;
        }
        _sequence = DOTween.Sequence();
        if (currentTimeScale > newTimeScale)
        {
            _sequence.Append(animateTransform.DOScale(_originalScale * (1f - scaleAnimationOffset), animationDuration).SetEase(Ease.OutSine));
            _sequence.Append(animateTransform.DOScale(_originalScale, animationDuration).SetEase(Ease.OutSine));
            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                _sequence.InsertCallback(0f, () => { material.EnableKeyword("_EMISSION"); });
                _sequence.Insert(0f, material.DOColor(_emissionColor, "_EmissionColor", animationDuration).SetEase(Ease.OutSine));
                _sequence.Insert(animationDuration, material.DOColor(Color.black, "_EmissionColor", animationDuration).SetEase(Ease.OutSine));
                _sequence.InsertCallback(animationDuration * 2f, () => { material.DisableKeyword("_EMISSION"); });
            }
        }
        else
        {
            _sequence.Append(animateTransform.DOScale(_originalScale * (1f + scaleAnimationOffset), animationDuration).SetEase(Ease.OutSine));
            _sequence.Append(animateTransform.DOScale(_originalScale, animationDuration).SetEase(Ease.OutSine));
            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                _sequence.InsertCallback(0f, () => { material.EnableKeyword("_EMISSION"); });
                _sequence.Insert(0f, material.DOColor(_emissionColor, "_EmissionColor", animationDuration).SetEase(Ease.OutSine));
                _sequence.Insert(animationDuration, material.DOColor(Color.black, "_EmissionColor", animationDuration).SetEase(Ease.OutSine));
                _sequence.InsertCallback(animationDuration * 2f, () => { material.DisableKeyword("_EMISSION"); });
            }
        }
        _sequence.Play();
        currentTimeScale = newTimeScale;
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
