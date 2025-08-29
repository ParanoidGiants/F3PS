using System;
using System.Linq;

public class DoorEventController
{
    public DoorsData Data { get; private set; }

    public Action<string> OnDoorOpened;
    public void UpdateDoorOpened(string id)
    {
        var doorData = Data.Doors.First(x => x.Id == id);
        doorData.IsOpen = true;
        OnDoorOpened?.Invoke(id);
    }

    public void InitializeData(DoorsData doorsData)
    {
        Data = doorsData;
        foreach (var doorData in Data.Doors)
        {
            if (doorData.IsOpen)
            {
                UpdateDoorOpened(doorData.Id);
            }
        }
    }
}
