using DG.Tweening;
using StarterAssets;
using UnityEngine;

public class TimeBubbleGrenadeProjectile : BaseProjectile
{
    [Header("References")]
    public Transform userSpace;
    public TimeBubble timeBubble;
    public float animationDuration = 0.5f;
    public float targetScale = 20f;
    private bool _isActive = false;

    public float Gravity => timeObject.gravityScale * Physics.gravity.magnitude;
    public float LifeTimePercentage => lifeTime / maximumLifeTimer;

    private void Update()
    {
        if (!_isHit || !_isActive) return;

        lifeTime += Time.deltaTime;
        if (lifeTime > maximumLifeTimer)
        {
            DeactivateTimeBubble();
        }
    }

    private void ActivateTimeBubble()
    {
        timeBubble.gameObject.SetActive(true);
        timeBubble.transform.localScale = Vector3.zero;
        timeBubble.transform
            .DOScale(Vector3.one * targetScale, animationDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
             {
                 _isActive = true;
             });
    }

    private void DeactivateTimeBubble()
    {
        _isActive = false;
        timeBubble.gameObject.transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                // Deactivate the GameObject after the tween is complete.
                timeBubble.gameObject.SetActive(false);
                gameObject.SetActive(false);
            });
    }
    
    override
    public void BeforeSetActive(Vector3 position, Vector3 targetPosition, float shootSpeed)
    {
        transform.SetParent(userSpace);
        timeObject.enabled = true;
        base.BeforeSetActive(position, targetPosition, shootSpeed);
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        col.enabled = true;
        timeBubble.gameObject.SetActive(false);
    }
    
    override
    public void SetHit()
    {
        if (Hit) return;
        
        _isHit = true;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        col.enabled = false;
        GetComponent<TimeObject>().enabled = false;
        ActivateTimeBubble();
    }

}
