using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class AnimateShieldMesh : Hittable
{
    [Header("Settings")]
    private Vector3 _originalScale;
    private float _animationTime = 0f;
    private Sequence _hitFlashSequence;

    public Renderer shieldRenderer;
    public Transform shieldTransform;
    public float animationDuration = 0.5f;
    public float animationScale = 1.5f;
    public Color hitFlashEmissionColor = Color.white;
    public Color currentHitFlashEmissionColor = Color.black;
    public float hitFlashEaseInAnimationDuration = 0.1f;
    public float hitFlashEaseOutAnimationDuration = 0.1f;
    public Ease hitFlashEaseIn = Ease.InSine;
    public Ease hitFlashEaseOut = Ease.OutSine;
    public bool setEmission = false;
    public float timeScale = 1f;

    private void Awake()
    {
        _originalScale = shieldTransform.localScale;
    }

    public override void OnHit(int damage, Vector3 hitDirection)
    {
        // Handle hit logic here, e.g., play sound or visual effects
        Debug.Log($"Shield hit with damage: {damage}");
        if (_hitFlashSequence != null && _hitFlashSequence.IsActive() && _hitFlashSequence.IsPlaying())
        {
            _hitFlashSequence.Kill();
        }
        _hitFlashSequence = DOTween.Sequence();
        shieldRenderer.material.EnableKeyword("_EMISSION");
        _hitFlashSequence.Insert(
            0f,
            DOTween.To(
                () => currentHitFlashEmissionColor,
                x => {
                    currentHitFlashEmissionColor = x;
                    shieldRenderer.material.SetColor("_EmissionColor", currentHitFlashEmissionColor);
                },
                hitFlashEmissionColor,
                hitFlashEaseInAnimationDuration
            )
            .SetEase(hitFlashEaseIn)
        );

        _hitFlashSequence.Insert(
            hitFlashEaseInAnimationDuration,
            shieldTransform.DOScale(_originalScale * animationScale, animationDuration)
                .SetEase(Ease.OutBack)
        );

        _hitFlashSequence.Insert(
            hitFlashEaseInAnimationDuration,
            DOTween.To(
                () => currentHitFlashEmissionColor,
                x => {
                    currentHitFlashEmissionColor = x;
                    shieldRenderer.material.SetColor("_EmissionColor", currentHitFlashEmissionColor);
                },
                Color.black,
                hitFlashEaseInAnimationDuration
        )
            .SetEase(hitFlashEaseOut)
        );

        _hitFlashSequence.Insert(
            hitFlashEaseInAnimationDuration + animationDuration,
            shieldTransform.DOScale(_originalScale, animationDuration)
                .SetEase(Ease.InBack)
        );

        _hitFlashSequence.OnComplete(() =>
        {
            shieldRenderer.material.DisableKeyword("_EMISSION");
        });
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
        shieldRenderer.material.DisableKeyword("_EMISSION");
    }
}
