using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Header("Settings")]
    public int numberOfPooledObjects = 20;
    private List<BaseProjectile> _projectiles;

    [Header("Watchers")]
    public bool isPlayer;

    public void Init(GameObject projectilePrefab, Transform user, bool isPlayer = false)
    {
        this.isPlayer = isPlayer;
        _projectiles = new List<BaseProjectile>();
        Transform parent = new GameObject("Projectiles" + projectilePrefab.name).transform;
        parent.transform.SetParent(user);
        for (int i = 0; i < numberOfPooledObjects; i++)
        {
            BaseProjectile projectile = Instantiate(projectilePrefab, parent).GetComponent<BaseProjectile>();
            projectile.Init(user.GetInstanceID(), isPlayer);
            projectile.gameObject.SetActive(false);
            _projectiles.Add(projectile.GetComponent<BaseProjectile>());
        }
    }

    public void ShootBullet(Vector3 fromPosition, Quaternion orientation, float shootSpeed)
    {
        foreach (BaseProjectile projectile in _projectiles)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                projectile.BeforeSetActive(fromPosition, orientation, shootSpeed);
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
