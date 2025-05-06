using DG.Tweening;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    [Header("Watchers")]
    public Renderer _renderer;
    public Color defaultColor;
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime;

    private void Awake()
    {
        InitReferences();
    }

    private void Start()
    {
        PitchTimeScale(currentTimeScale);
        defaultColor = _renderer.material.color;
    }

    protected virtual void InitReferences()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }
    }

    public virtual void PitchTimeScale(float newTimeScale)
    {
        if (currentTimeScale == newTimeScale)
        {
            return;
        }

        var targetColor = defaultColor * newTimeScale * newTimeScale;
        _renderer.material.DOColor(targetColor, newTimeScale * 0.5f).SetEase(Ease.OutCubic);

        currentTimeScale = newTimeScale;
    }

    protected virtual void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }
}
