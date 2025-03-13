using F3PS.Enemy;
using UnityEngine;

public class KillEnemyDoor : DoorController
{
    [Space(10)]
    [Header("Kill Enemy Reference")]
    public BaseEnemy enemy;
    public void Start()
    {
        enemy.Dead += OpenDoor;
    }
}
