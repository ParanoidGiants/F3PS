using DG.Tweening;
using UnityEngine;

public class RotateBackAndForthOverTime : MonoBehaviour
{
    public Transform from;
    public Transform to;
    public float duration = 1f;
    public Ease easeType = Ease.InOutSine;

    void Start()
    {
        transform.rotation = from.rotation;

        transform.DORotate(to.rotation.eulerAngles, duration, RotateMode.Fast)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
