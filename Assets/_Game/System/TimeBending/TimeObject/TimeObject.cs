using DG.Tweening;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime;
    public AnimateMesh animateMesh;


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
        animateMesh.TimeFlash(newTimeScale);
        animateMesh.SetTimeScale(newTimeScale);

        currentTimeScale = newTimeScale;
    }

    protected virtual void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }
}
