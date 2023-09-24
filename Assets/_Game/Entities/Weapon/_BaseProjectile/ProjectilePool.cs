using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public int numberOfPooledObjects = 20;
    private List<BaseProjectile> _projectiles;

    public void Init(GameObject projectilePrefab, Transform user)
    {
        _projectiles = new List<BaseProjectile>();
        Transform parent = new GameObject("Projectiles").transform;
        parent.transform.SetParent(user);
        for (int i = 0; i < numberOfPooledObjects; i++)
        {
            GameObject obj = Instantiate(projectilePrefab, parent);
            obj.GetComponent<BaseProjectile>().Init(user.GetInstanceID());
            obj.SetActive(false);
            _projectiles.Add(obj.GetComponent<BaseProjectile>());
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
