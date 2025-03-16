using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemySpace;

    public void Spawn()
    {
        enemySpace.SetActive(true);
        enemySpace.transform.parent = null;
        Destroy(gameObject);
    }
}
