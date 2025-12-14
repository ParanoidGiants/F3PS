using System;
using System.Linq;

public class EnemyEventController
{
    public EnemiesData Data { get; private set; }
    public void UpdateEnemyDied(string name)
    {
        var enemyData = Data.Enemies.First(x => x.name == name);
        enemyData.isAlive = false;
    }


    public void InitializeData(EnemiesData enemiesData)
    {
        Data = enemiesData;
    }
}
