using UnityEngine;
using F3PS.AI.Sensors;

public class AggressiveOnlySensorController : SensorController
{
    public override bool IsTargetInLineOfSight()
    {
        return aggressiveVision.IsTargetInSight;
    }

    public override void SetState(SensorState state)
    {
        state = SensorState.AGGRESSIVE;
        aggressiveVision.SetSensorState(SensorState.AGGRESSIVE);
        aggressiveMovement.SetSensorState(SensorState.AGGRESSIVE);
    }

    public override Hittable GetTargetFromSensors()
    {
        if (aggressiveMovement.HasTarget)
        {
            return aggressiveMovement.SelectedTarget;
        }
        return aggressiveVision.SelectedTarget;
    }

    public override bool HasTarget()
    {
        return aggressiveMovement.HasTarget || aggressiveVision.HasTarget;
    }
}
