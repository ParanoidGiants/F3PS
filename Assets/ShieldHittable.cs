using DG.Tweening;
using UnityEngine;

public class ShieldHittable : Hittable
{
    private Vector3 _originalScale;
    private Sequence _hitFlashSequence;

    public Transform shieldTransform;
    [Header("Settings")]
    public float animationScale = 1.5f;
    public float hitFlashEaseInAnimationDuration = 0.1f;
    public float hitFlashEaseOutAnimationDuration = 0.1f;
    public Ease hitFlashEaseIn = Ease.InSine;
    public Ease hitFlashEaseOut = Ease.OutSine;
    public float timeScale = 1f;

    private void Awake()
    {
        _originalScale = shieldTransform.localScale;
    }

    public override void OnHit(int damage, Vector3 hitDirection)
    {
        Debug.Log($"Shield hit with damage: {damage}");
        if (_hitFlashSequence != null && _hitFlashSequence.IsActive() && _hitFlashSequence.IsPlaying())
        {
            _hitFlashSequence.Kill();
        }
        _hitFlashSequence = DOTween.Sequence();
        _hitFlashSequence.Insert(
            0f,
            shieldTransform.DOScale(_originalScale * animationScale, hitFlashEaseInAnimationDuration)
                .SetEase(hitFlashEaseIn)
        );
        _hitFlashSequence.Insert(
            hitFlashEaseInAnimationDuration,
            shieldTransform.DOScale(_originalScale, hitFlashEaseOutAnimationDuration)
                .SetEase(hitFlashEaseOut)
        );
        _hitFlashSequence.timeScale = timeScale;
        _hitFlashSequence.Play();
    }

    private void OnDisable()
    {
        if (_hitFlashSequence != null && _hitFlashSequence.IsActive() && _hitFlashSequence.IsPlaying())
        {
            _hitFlashSequence.Kill();
        }
        shieldTransform.localScale = _originalScale;
    }
}
