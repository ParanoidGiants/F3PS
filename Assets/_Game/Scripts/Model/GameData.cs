using System;
using System.Linq;

[Serializable]
public class GameData
{
    public PlayerData PlayerData;
    public SwitchesData SwitchesData;
    public EnemyData[] EnemiesData;

    public void RegisterEnemy(int instanceId)
    {
        var enemyData = new EnemyData();
        enemyData.instanceId = instanceId;
        enemyData.isAlive = true;
        EnemiesData = EnemiesData.Append(enemyData).ToArray();
    }

    public void KillEnemy(int instanceId)
    {
        var enemyData = EnemiesData.First(x => x.instanceId == instanceId);
        enemyData.isAlive = false;
    }

    public void RegisterAllEnemies(int[] enemiesInstanceIds)
    {
        foreach (var instanceId in enemiesInstanceIds)
        {
            RegisterEnemy(instanceId);
        }
    }
}
