using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{

    [Header("Reference")]
    public HittableManager hittableManager;

    [Header("Settings")]
    public int numberOfPooledObjects = 20;
    private List<BaseProjectile> _projectiles;


    public void Init(GameObject projectilePrefab, Transform userSpace)
    {
        _projectiles = new List<BaseProjectile>();
        Transform parent = new GameObject("Projectiles" + projectilePrefab.name).transform;
        parent.transform.SetParent(userSpace);
        for (int i = 0; i < numberOfPooledObjects; i++)
        {
            BaseProjectile projectile = Instantiate(projectilePrefab, parent).GetComponent<BaseProjectile>();
            projectile.Init(userSpace.GetInstanceID(), hittableManager);
            projectile.gameObject.SetActive(false);
            _projectiles.Add(projectile.GetComponent<BaseProjectile>());
        }
    }

    public void ShootBullet(Vector3 fromPosition, Vector3 targetPosition, float shootSpeed)
    {
        foreach (BaseProjectile projectile in _projectiles)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                projectile.BeforeSetActive(fromPosition, targetPosition, shootSpeed);
                projectile.gameObject.SetActive(true);
                return;
            }
        }
        
        Debug.LogWarning("There are no available bullets in pool!");
    }
    
    public GameObject GetBullet()
    {
        foreach (BaseProjectile projectile in _projectiles)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                projectile.gameObject.SetActive(true);
                return projectile.gameObject;
            }
        }
        return null;
    }
}
