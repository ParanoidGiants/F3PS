using System;
using System.Linq;

public class EnemyEventController
{
    public EnemiesData Data { get; private set; }
    public Action<string> OnEnemyDied;
    public void UpdateEnemyDied(string name)
    {
        var enemyData = Data.Enemies.First(x => x.name == name);
        enemyData.isAlive = false;
        OnEnemyDied?.Invoke(name);
    }


    public void InitializeData(EnemiesData enemiesData)
    {
        Data = enemiesData;
        foreach (var enemyData in Data.Enemies.Where(x => !x.isAlive))
        {
            OnEnemyDied?.Invoke(enemyData.name);
        }
    }
}
