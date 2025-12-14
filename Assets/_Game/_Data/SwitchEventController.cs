using System;
using System.Linq;

public class SwitchEventController
{
    public SwitchesData Data { get; private set; }

    public Action<string> OnSwitchTriggered;
    public void UpdateSwitchTriggered(string id)
    {
        var switchData = Data.Switches.First(x => x.Id == id);
        switchData.IsTriggered = true;
        OnSwitchTriggered?.Invoke(id);
    }

    public void InitializeData(SwitchesData switchesData)
    {
        Data = switchesData;
        foreach (var switchData in Data.Switches.Where(x => x.IsTriggered))
        {
            UpdateSwitchTriggered(switchData.Id);
        }
    }
}