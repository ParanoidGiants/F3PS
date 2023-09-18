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
            defaultVision.SetAggressive(state);
            searchingVision.SetAggressive(state);
            aggressiveMovement.SetAggressive(state);
            
            if (state == SensorState.AGGRESSIVE)
            {
                defaultVision.SetActive(false);
                
                searchingVision.SetActive(true);
                aggressiveMovement.SetActive(true);
            }
            else if (state == SensorState.SEARCHING)
            {
                defaultVision.SetActive(false);
                aggressiveMovement.SetActive(false);
                
                searchingVision.SetActive(true);
            }
            else
            {
                searchingVision.SetActive(false);
                aggressiveMovement.SetActive(false);
                
                defaultVision.SetActive(true);
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
            else if (state == SensorState.SEARCHING)
            {
                return searchingVision.SelectedTarget;
            }
            else // state is aggressive
            if (searchingVision.HasTarget)
            {
                if (
                    aggressiveMovement.HasTarget
                    && aggressiveMovement.SelectedTarget.damageMultiplier > searchingVision.SelectedTarget.damageMultiplier
                )
                {
                    return aggressiveMovement.SelectedTarget;
                }
                else
                {
                    return searchingVision.SelectedTarget;
                }
            }
            else return aggressiveMovement.SelectedTarget;
        }
    }
}
