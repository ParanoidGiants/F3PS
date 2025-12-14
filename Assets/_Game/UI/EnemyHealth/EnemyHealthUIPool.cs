using UnityEngine;
using F3PS.AI;
using System.Collections.Generic;
using System;

namespace F3PS.Enemy.UI
{
    public class EnemyHealthUIPool : MonoBehaviour
    {
        public GameObject healthUIPrefab;
        public Dictionary<Transform, EnemyHealthUI> healthUIs = new Dictionary<Transform, EnemyHealthUI>();

        public void CreateEnemyHealthUI(Transform target)
        {
            var healthUI = Instantiate(healthUIPrefab, transform.parent).GetComponent<EnemyHealthUI>();
            healthUI.SetTarget(target);
            healthUIs.Add(target, healthUI);
            healthUI.gameObject.SetActive(false);
        }

        public void RemoveEnemyHealthUI(Transform target)
        {
            if (healthUIs.TryGetValue(target, out var healthUI))
            {
                Destroy(healthUI.gameObject);
                healthUIs.Remove(target);
            }
        }

        public void OnHitTarget(Transform target, int health, int maxHealth)
        {
            if (!healthUIs.TryGetValue(target, out var healthUI))
            {
                Debug.LogWarning($"No health UI found for target {target.name}.");
                return;
            }
            healthUI.SetFill((float) health / maxHealth);
            if (!healthUI.gameObject.activeSelf)
            {
                healthUI.gameObject.SetActive(true);
            }
        }

        public BossHealthUI bossHealthUI;
        public void EnableBossUI()
        {
            bossHealthUI.gameObject.SetActive(true);
        }
        
        public void DisableBossUI()
        {
            bossHealthUI.gameObject.SetActive(false);
        }

        public void OnHitBoss(int health, int maxHealth)
        {
            bossHealthUI.SetFill(health / (float)maxHealth);
        }
    }
}
