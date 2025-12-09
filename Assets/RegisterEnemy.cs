using System.Linq;
using F3PS;
using UnityEngine;

public class RegisterEnemy : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance.saveGameManager.GameData.EnemiesData.Enemies.Any(e => e.name == gameObject.name && !e.isAlive))
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.saveGameManager.EnemyEventController.UpdateEnemyDied(gameObject.name);
    }
}
