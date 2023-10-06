using UnityEngine;

public class GravityProjectile : BaseProjectile
{
    [SerializeField] private TimeZone _timeZone;
    override
    public void BeforeSetActive(Vector3 position, Quaternion rotation, float shootSpeed)
    {
        base.BeforeSetActive(position, rotation, shootSpeed);
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.None;
        _timeZone.gameObject.SetActive(false);
    }
    
    override
    public void SetHit()
    {
        if (Hit) return;
        
        _isHit = true;
        _rb.isKinematic = true;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<TimeObject>().enabled = false;
        _timeZone.gameObject.SetActive(true);
        StartCoroutine(SetInactiveAfterSeconds(removeAfterSeconds));
    }

}
