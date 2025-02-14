using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class HittableManager : MonoBehaviour
{
    [Header("References")]
    public Renderer hittableRenderer;
    public CinemachineImpulseSource shakeSource;

    [Space(10)]
    [Header("Shake Settings")]
    public float shakePower;

    [Space(10)]
    [Header("Flash Settings")]
    public bool flash = false;
    public Color _emissionColor;
    public float flashEaseInAnimationDuration;
    public float flashEaseOutAnimationDuration;
    public Ease easeIn;
    public Ease easeOut;


    [Space(10)]
    [Header("Watchers")]
    public Collider[] colliders;

    private Sequence _sequence;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    public void Shake(float damageMultiplier)
    {
        shakeSource.GenerateImpulse(damageMultiplier * shakePower);
    }

    public void Update()
    {
        if (flash)
        {
            flash = false;
            Flash();
        }
    }

    public void Flash()
    {
        if (_sequence != null && _sequence.IsActive() && _sequence.IsPlaying())
        {
            _sequence.Kill();
        }
        _sequence = DOTween.Sequence();
        foreach (var material in hittableRenderer.materials)
        {
            _sequence.InsertCallback(
                0f,
                () => { material.EnableKeyword(GlobalConstants.MATERIAL_KEYWORD_EMISSION); }
            );
            _sequence.Insert(
                0f,
                material.DOColor(
                    _emissionColor,
                    GlobalConstants.MATERIAL_KEYWORD_EMISSION_COLOR,
                    flashEaseInAnimationDuration
                )
                .SetEase(easeIn)
            );
            _sequence.Insert(
                flashEaseInAnimationDuration,
                material.DOColor(Color.black, GlobalConstants.MATERIAL_KEYWORD_EMISSION_COLOR, flashEaseOutAnimationDuration)
                .SetEase(easeOut)
            );
            _sequence.InsertCallback(
                flashEaseInAnimationDuration + flashEaseOutAnimationDuration,
                () => { material.DisableKeyword(GlobalConstants.MATERIAL_KEYWORD_EMISSION); }
            );
        }
        _sequence.Play();
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
