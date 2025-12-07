using System;
using System.Linq;

[Serializable]
public class EnemiesData
{
    public EnemyData[] Enemies;

    public void Initialize(string[] gameObjectNames)
    {
        Enemies = new EnemyData[0];
        foreach (var gameObjectName in gameObjectNames)
        {
            var doorData = new EnemyData
            {
                name = gameObjectName,
                isAlive = true
            };
            Enemies = Enemies.Append(doorData).ToArray();
        }
    }

    public void KillEnemy(string name)
    {
        var enemyData = Enemies.First(x => x.name == name);
        enemyData.isAlive = false;
    }
}
