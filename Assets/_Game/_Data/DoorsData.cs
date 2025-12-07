using System;
using System.Linq;

[Serializable]
public class DoorsData
{
    public DoorData[] Doors;

    public void Initialize(string[] doorsIds)
    {
        Doors = new DoorData[0];
        foreach (var id in doorsIds)
        {
            var doorData = new DoorData
            {
                Id = id,
                IsOpen = false
            };
            Doors = Doors.Append(doorData).ToArray();
        }
    }
}
