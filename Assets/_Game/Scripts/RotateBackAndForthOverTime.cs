using DG.Tweening;
using UnityEngine;

public class RotateBackAndForthOverTime : MonoBehaviour
{
    [Header("Debug")]
    public Rigidbody _rigidbody;
    public Tween _tween;
    public TimeObject _timeObject;

    [Space(10)]
    [Header("References")]
    public Transform from;
    public Transform to;

    [Space(10)]
    [Header("Settings")]
    public float duration = 1f;
    public Ease easeType = Ease.InOutSine;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _timeObject = GetComponentInChildren<TimeObject>();
    }

    void Start()
    {
        _rigidbody.rotation = from.rotation;
        _tween = _rigidbody.DORotate(to.rotation.eulerAngles, duration, RotateMode.Fast)
            .SetEase(easeType)
            .SetUpdate(UpdateType.Fixed)
            .SetLoops(-1, LoopType.Yoyo);
        _timeObject.OnTimeScaleChanged += OnTimeScaleChanged;
    }

    private void OnTimeScaleChanged(float timeScale)
    {
        _tween.timeScale = timeScale;
    }
}
