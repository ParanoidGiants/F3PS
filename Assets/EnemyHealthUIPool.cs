using Enemy;
using UnityEngine;

public class EnemyHealthUIPool : MonoBehaviour
{
    public EnemyHealthUI enemyHealthUI;

    private void Start()
    {
        enemyHealthUI.gameObject.SetActive(false);
    }

    public void OnHitTarget(BaseEnemy target)
    {
        if (enemyHealthUI.target != target.transform)
        {
            enemyHealthUI.gameObject.SetActive(true);
            enemyHealthUI.SetTarget(target.transform);
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
