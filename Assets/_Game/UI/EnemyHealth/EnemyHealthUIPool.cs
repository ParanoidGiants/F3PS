using UnityEngine;
using F3PS.AI;

namespace F3PS.Enemy.UI
{
    public class EnemyHealthUIPool : MonoBehaviour
    {
        public EnemyHealthUI enemyHealthUI;
        public BossHealthUI bossHealthUI;

        private void Start()
        {
            enemyHealthUI.gameObject.SetActive(false);
        }

        public void OnHitTarget(Transform target, int health, int maxHealth)
        {
            var bodyTransform = target;
            if (enemyHealthUI.target != bodyTransform)
            {
                enemyHealthUI.gameObject.SetActive(true);
                enemyHealthUI.SetTarget(bodyTransform);
            }
            
            enemyHealthUI.SetFill((float) health / maxHealth);
        }
        
        public void OnKillTarget(Transform target)
        {
            if (enemyHealthUI.target != target) return;
            
            
            enemyHealthUI.SetFill(1);
            enemyHealthUI.SetTarget(null);
            enemyHealthUI.gameObject.SetActive(false);
        }
        
        public void OnHitBoss(BossEnemy boss)
        {
            bossHealthUI.SetFill(boss.health/ (float) boss.maxHealth);
        }
        
        public void EnableBossUI()
        {
            bossHealthUI.gameObject.SetActive(true);
        }
        
        public void DisableBossUI()
        {
            bossHealthUI.gameObject.SetActive(false);
        }
    }
}
