using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.Sensors
{
    public class SensorController : MonoBehaviour
    {
        public SensorState state;
        public VisionSensor defaultVision;
        public VisionSensor searchingVision;
        public MovementSensor aggressiveMovement;

        public void SetState(SensorState state)
        {
            this.state = state;
            defaultVision.SetSensorState(state);
            searchingVision.SetSensorState(state);
            
            aggressiveMovement.SetSensorState(state);
            
            if (state == SensorState.AGGRESSIVE)
            {
                defaultVision.SetActive(false);
                searchingVision.SetActive(true);
                
                aggressiveMovement.SetActive(true);
            }
            else if (state == SensorState.SEARCHING)
            {
                defaultVision.SetActive(false);
                searchingVision.SetActive(true);
                
                aggressiveMovement.SetActive(false);
            }
            else
            {
                defaultVision.SetActive(true);
                searchingVision.SetActive(false);
                
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

            return searchingVision.IsTargetInSight();
        }

        public Hittable GetTargetFromSensors()
        {
            if (state == SensorState.IDLE)
            {
                return defaultVision.SelectedTarget;
            }
            if (state == SensorState.SEARCHING)
            {
                return searchingVision.SelectedTarget;
            }
            // else state is aggressive
            if (searchingVision.HasTarget && aggressiveMovement.HasTarget)
            {
                return aggressiveMovement.SelectedTarget;
            }
            if (searchingVision.HasTarget)
            {
                // else work with what you see
                return searchingVision.SelectedTarget;
            }
            
            return aggressiveMovement.SelectedTarget;
        }
    }
}
