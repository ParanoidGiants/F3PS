using System;
using System.Linq;

[Serializable]
public class GameData
{
    public PlayerData PlayerData;
    public PlayerEventController PlayerEventController;
    public EnemyData[] EnemiesData;
    public SwitchData[] SwitchesData;

    public GameData()
    {
        PlayerData = new PlayerData();
        PlayerEventController = new PlayerEventController();
    }

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

    public void RegisterSwitch(int instanceId)
    {
        var switchData = new SwitchData();
        switchData.instanceId = instanceId;
        switchData.isOn = false;
        SwitchesData = SwitchesData.Append(switchData).ToArray();

    }
    public void SwitchOn(int instanceId)
    {
        var switchData = SwitchesData.First(x => x.instanceId == instanceId);
        switchData.isOn = true;
    }

    public void RegisterAllEnemies(int[] enemiesInstanceIds)
    {
        foreach (var instanceId in enemiesInstanceIds)
        {
            RegisterEnemy(instanceId);
        }
    }

    public void RegisterAllSwitches(int[] switchesInstanceIds)
    {
        foreach (var instanceId in switchesInstanceIds)
        {
            RegisterSwitch(instanceId);
        }
    }
}
