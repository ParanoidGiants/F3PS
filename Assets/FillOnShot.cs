using UnityEngine;
using UnityEngine.Events;

public class FillOnShot : MonoBehaviour
{
    public float fill = 0f;
    public float fillPerProjectile = 0.2f;
    public float unfillPerSecond = 0.2f;
    public MeshRenderer liquidRenderer;
    public TimeObject timeObject;
    public bool isFilled = false;
    public UnityEvent isFilledEvent;

    private void OnCollisionEnter(Collision collision)
    {
        if (isFilled)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<OsirisKickProjectile>(out var _) || collision.gameObject.TryGetComponent<HorusPalmProjectile>(out var _))
        {
            fill += fillPerProjectile;
            fill = Mathf.Clamp01(fill);
            liquidRenderer.material.SetFloat("_Fill", fill);
            if (fill == 1f)
            {
                isFilled = true;
                isFilledEvent.Invoke();
            }
        }
    }

    private void Update()
    {
        if (isFilled)
        {
            return;
        }

        fill -= timeObject.ScaledDeltaTime * unfillPerSecond;
        fill = Mathf.Clamp01(fill);
        liquidRenderer.material.SetFloat("_Fill", fill);
    }
}
