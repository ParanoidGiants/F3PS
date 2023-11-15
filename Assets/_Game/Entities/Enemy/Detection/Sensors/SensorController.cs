using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.Sensors
{
    public class SensorController : MonoBehaviour
    {
        public SensorState state;
        public VisionSensor defaultVision;
        public VisionSensor aggressiveVision;
        public MovementSensor aggressiveMovement;

        public void SetState(SensorState state)
        {
            this.state = state;
            defaultVision.SetSensorState(state);
            aggressiveVision.SetSensorState(state);
            
            aggressiveMovement.SetSensorState(state);
            
            if (state == SensorState.AGGRESSIVE)
            {
                defaultVision.SetActive(false);
                aggressiveVision.SetActive(true);
                
                aggressiveMovement.SetActive(true);
            }
            else if (state == SensorState.SEARCHING)
            {
                defaultVision.SetActive(false);
                aggressiveVision.SetActive(true);
                
                aggressiveMovement.SetActive(false);
            }
            else
            {
                defaultVision.SetActive(true);
                aggressiveVision.SetActive(false);
                
                aggressiveMovement.SetActive(false);
            }
        }
        
        public bool IsTargetDetected()
        {
            if (state == SensorState.IDLE)
            {
                return defaultVision.IsTargetInSight();
            }
            
            if (state == SensorState.AGGRESSIVE && aggressiveMovement.HasTarget)
            {
                return true;
            }

            return aggressiveVision.IsTargetInSight();
        }

        public Hittable GetTargetFromSensors()
        {
            if (state == SensorState.IDLE)
            {
                return defaultVision.SelectedTarget;
            }
            if (state == SensorState.SEARCHING)
            {
                return aggressiveVision.SelectedTarget;
            }
            // else state is aggressive
            if (aggressiveMovement.HasTarget)
            {
                return aggressiveMovement.SelectedTarget;
            }
            if (aggressiveVision.HasTarget)
            {
                return aggressiveVision.SelectedTarget;
            }
            
            return aggressiveMovement.SelectedTarget;
        }
    }
}
