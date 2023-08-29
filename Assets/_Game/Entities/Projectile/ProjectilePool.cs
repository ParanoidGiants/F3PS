using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public int numberOfPooledObjects = 20;
    private List<Projectile> _projectiles;

    public void Init(GameObject projectilePrefab, int attackerId)
    {
        _projectiles = new List<Projectile>();
        for (int i = 0; i < numberOfPooledObjects; i++)
        {
            GameObject obj = Instantiate(projectilePrefab, transform);
            obj.GetComponent<Projectile>().Init(attackerId);
            obj.SetActive(false);
            _projectiles.Add(obj.GetComponent<Projectile>());
        }
    }

    public void ShootBullet(Vector3 fromPosition, Quaternion orientation, float shootSpeed)
    {
        foreach (Projectile projectile in _projectiles)
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
        foreach (Projectile projectile in _projectiles)
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
