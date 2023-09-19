using UnityEngine;
using F3PS.AI;

namespace F3PS.Enemy.UI
{
    public class EnemyHealthUIPool : MonoBehaviour
    {
        public EnemyHealthUI enemyHealthUI;

        private void Start()
        {
            enemyHealthUI.gameObject.SetActive(false);
        }

        public void OnHitTarget(BaseEnemy target)
        {
            var bodyTransform = target.body.transform;
            if (enemyHealthUI.target != bodyTransform)
            {
                enemyHealthUI.gameObject.SetActive(true);
                enemyHealthUI.SetTarget(bodyTransform);
            }
            
            enemyHealthUI.SetFill(target.health/ (float) target.maxHealth);
        }
        
        public void OnKillTarget(Transform target)
        {
            if (enemyHealthUI.target != target) return;
            
            
            enemyHealthUI.SetFill(1);
            enemyHealthUI.SetTarget(null);
            enemyHealthUI.gameObject.SetActive(false);
        }
    }
}
