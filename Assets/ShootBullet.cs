using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    public Transform bulletPool;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float shootAgainTimer = 0.2f;
    
    private float shootAgainTime = 0.0f;
    private Camera _cam;
    
    void Start()
    {
        _cam = Camera.main;
    }
    
    void Update()
    {
        if (shootAgainTime > 0f)
        {
            shootAgainTime -= Time.deltaTime;
        }
        else
        {
            shootAgainTime = 0f;
        }
    }
        
    public void OnShoot()
    {
        if (shootAgainTime != 0f) return;
        
        shootAgainTime = shootAgainTimer;
        GameObject bullet = Instantiate(bulletPrefab, bulletPool);
        bullet.transform.position = bulletSpawn.position;
        bullet.transform.rotation = _cam.transform.rotation;
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
    }
}
