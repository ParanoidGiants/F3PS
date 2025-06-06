using DG.Tweening;
using UnityEngine;

public class MeshAnimateHit : MonoBehaviour
{
    [Header("References")]
    public Renderer[] renderers;

    [Space(20)]
    [Header("Hit Flash Animation Settings")]
    public Color hitFlashEmissionColor;
    public Color currentHitFlashEmissionColor = Color.black;
    public float hitFlashEaseInAnimationDuration;
    public float hitFlashEaseOutAnimationDuration;
    public Ease hitFlashEaseIn;
    public Ease hitFlashEaseOut;
    private Sequence _hitFlashSequence;


    private bool setEmission = false;
    private Color CurrentEmissionColor => currentHitFlashEmissionColor;

    [Space(10)]
    [Header("Debug")]
    public bool hitFlashAnimate = false;
    public float timeScale = 1f;

    private void Awake()
    {
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.EnableKeyword(GlobalConstants.MATERIAL_KEYWORD_EMISSION);
            }
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
    }

    private void OnDestroy()
    {
        if (_hitFlashSequence != null)
        {
            _hitFlashSequence.Kill();
            _hitFlashSequence = null;
        }
    }

    private void SetEmission(Color color)
    {
        foreach (var renderer in renderers)
        {

            foreach (var material in renderer.materials)
            {
                material.SetColor(GlobalConstants.MATERIAL_KEYWORD_EMISSION_COLOR, color);
            }
        }
    }

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
}
