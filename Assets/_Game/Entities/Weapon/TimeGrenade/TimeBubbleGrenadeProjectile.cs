using StarterAssets;
using UnityEngine;

public class TimeBubbleGrenadeProjectile : BaseProjectile
{
    [Header("References")]
    public TimeBubble timeBubble;
    public new Collider collider;

    public float Gravity => _timeObject.gravityScale * Physics.gravity.magnitude;
    public float LifeTimePercentage => lifeTime / maximumLifeTime;

    private void Update()
    {
        if (!_isHit) return;
        
        lifeTime += Time.deltaTime;
        if (lifeTime > maximumLifeTime)
        {
            gameObject.SetActive(false);
        }
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
        timeBubble.gameObject.SetActive(true);
    }

}
