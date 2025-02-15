using DG.Tweening;
using UnityEngine;

public class HittableFlash : MonoBehaviour
{
    private Sequence _sequence;

    [Header("References")]
    public Renderer hittableRenderer;
    
    [Space(10)]
    [Header("Flash Settings")]
    public bool flash = false;
    public Color _emissionColor;
    public float flashEaseInAnimationDuration;
    public float flashEaseOutAnimationDuration;
    public Ease easeIn;
    public Ease easeOut;

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
