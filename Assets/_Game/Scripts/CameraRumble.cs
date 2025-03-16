using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraRumble : MonoBehaviour
{
    #region DEBUG
    [Header("Debug")]
    public bool debug = false;
    public bool triggerRumble = false;
    public float rumbleDuration = 5f;
    public void Update()
    {
        if (debug && triggerRumble)
        {
            triggerRumble = false;
            StartCoroutine(TriggerImpulse(rumbleDuration));
        }
    }
    #endregion DEBUG

    [Space(10)]
    [Header("Settings")]
    public Vector2 rumbleIntensityRandomRange = new Vector2(1f, 2f);


    private CinemachineImpulseSource impulseSource;
    private Coroutine coroutine;
    void Awake() { impulseSource = GetComponent<CinemachineImpulseSource>(); }
    
    public void TriggerRumble(float duration)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(TriggerImpulse(duration));
    }

    private IEnumerator TriggerImpulse(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float randomIntensity = Random.Range(rumbleIntensityRandomRange.x, rumbleIntensityRandomRange.y);
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            impulseSource.GenerateImpulse(randomIntensity*randomDirection);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
