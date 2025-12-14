using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class SwitchesData
{
    public SwitchData[] Switches;

    public void Initialize(string[] switchesIds)
    {
        Switches = new SwitchData[0];
        foreach (var id in switchesIds)
        {
            var switchData = new SwitchData
            {
                Id = id,
                IsTriggered = false
            };
            Switches = Switches.Append(switchData).ToArray();
        }
    }
}
