using DG.Tweening;
using StarterAssets;
using UnityEngine;

public class TimeBubbleGrenadeProjectile : BaseProjectile
{
    [Header("References")]
    public TimeBubble timeBubble;
    public new Collider collider;
    public float animationDuration = 0.5f;
    public float targetScale = 20f;
    private bool _isActive = false;

    public float Gravity => _timeObject.gravityScale * Physics.gravity.magnitude;
    public float LifeTimePercentage => lifeTime / maximumLifeTime;

    private void Update()
    {
        if (!_isHit || !_isActive) return;

        lifeTime += Time.deltaTime;
        if (lifeTime > maximumLifeTime)
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
    public void InitReferences()
    {
        base.InitReferences();
        transform.parent = FindObjectOfType<StarterAssetsInputs>().transform;
    }
    
    override
    public void BeforeSetActive(Vector3 position, Quaternion rotation, float shootSpeed)
    {
        _timeObject.enabled = true;

        base.BeforeSetActive(position, rotation, shootSpeed);
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.None;
        collider.enabled = true;
        timeBubble.gameObject.SetActive(false);
    }
    
    override
    public void SetHit()
    {
        if (Hit) return;
        
        _isHit = true;
        _rb.isKinematic = true;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        collider.enabled = false;
        GetComponent<TimeObject>().enabled = false;
        ActivateTimeBubble();
    }

}
