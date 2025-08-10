using System.Collections.Generic;
using F3PS;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    private PlayerData PlayerData => GameManager.Instance.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;
    public List<SpawnPoint> spawnPoints;

    private void OnEnable()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            spawnPoints[i].index = i;
            spawnPoints[i].OnSpawnPointEntered += OnSpawnPointEntered;
        }
    }

    private void OnDisable()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            spawnPoint.OnSpawnPointEntered -= OnSpawnPointEntered;
        }
    }

    private void OnSpawnPointEntered(int index)
    {
        if (PlayerData.CurrentSpawnPoint < index)
        {
            PlayerEventController.UpdateCurrentSpawnPoint(index);
        }
    }

    public Transform GetCurrentSpawnPosition()
    {
        return spawnPoints[PlayerData.CurrentSpawnPoint].transform;
    }
}
