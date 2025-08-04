using F3PS.Damage.Take;
 
namespace F3PS.AI.Sensors
{
    public class BossSensorController : SensorController
    {

        private void Awake()
        {
            state = SensorState.AGGRESSIVE;
            aggressiveMovement.SetSensorState(state);
        }

        override
        public void SetState(SensorState state) { }

        override
        public bool HasTarget()
        {
            return aggressiveMovement.HasTarget;
        }
        
        override 
        public Hittable GetTargetFromSensors()
        {   
            return aggressiveMovement.SelectedTarget;
        }
        
        override
        public bool IsTargetInLineOfSight()
        {
            return true;
        }
    }
}
