using System.Collections.Generic;
using UnityEngine;

public class YaggiSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject yaggiPrefab;

    private Dictionary<OnEnemyDied, Transform> enemyDiedSpawnPoints = new();

    private void Start()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            SpawnEnemyAt(spawnPoint);
        }
    }

    private void SpawnEnemyAt(Transform spawnPoint)
    {
        Debug.Log(spawnPoint.name);
        Debug.Log(transform.name);
        var enemy = Instantiate(yaggiPrefab, Vector3.zero, Quaternion.identity, transform);
        var yaggi = enemy.GetComponentInChildren<FromSpawnerYaggiStandardController>();
        yaggi.navMeshAgent.Warp(spawnPoint.position);
        var onEnemyDied = enemy.GetComponent<OnEnemyDied>();
        enemyDiedSpawnPoints.Add(onEnemyDied, spawnPoint);
        onEnemyDied.OnEnemyDiedEvent += RespawnHandler;
        Debug.Log("SPAWNED");
    }

    void RespawnHandler(GameObject oldEnemy)
    {
        var oldEnemyDied = oldEnemy.GetComponent<OnEnemyDied>();
        var spawnPoint = enemyDiedSpawnPoints[oldEnemyDied];
        oldEnemyDied.OnEnemyDiedEvent -= RespawnHandler;
        enemyDiedSpawnPoints.Remove(oldEnemyDied);

        SpawnEnemyAt(spawnPoint);
    }

    private void OnDestroy()
    {
        foreach (var enemyDied in enemyDiedSpawnPoints.Keys)
        {
            enemyDied.OnEnemyDiedEvent -= RespawnHandler;
        }
    }
}
