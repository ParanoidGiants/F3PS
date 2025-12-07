using F3PS;
using UnityEngine;

public class RegisterEnemy : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.saveGameManager.EnemyEventController.OnEnemyDied += OnKillEnemy;
    }

    private void OnDisable()
    {
        GameManager.Instance.saveGameManager.EnemyEventController.OnEnemyDied -= OnKillEnemy;
    }

    private void OnKillEnemy(string name)
    {
        if (gameObject.name == name)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.saveGameManager.EnemyEventController.UpdateEnemyDied(gameObject.name);
    }
}
