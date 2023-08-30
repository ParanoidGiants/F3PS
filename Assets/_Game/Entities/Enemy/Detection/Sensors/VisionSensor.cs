using UnityEngine;

namespace F3PS.AI.Sensors
{
    public class VisionSensor : BaseSensor
    {
        [Space(10)]
        [Header("Reference")]
        public Transform eyes;

        public bool IsTargetInSight()
        {
            if (!HasTarget) return false;
            
            var position = eyes.position;
            var targetPosition1 = SelectedTarget.Center();
            var direction1 = targetPosition1 - position;
            var playerDistance1 = direction1.magnitude;
                
            direction1.Normalize();
            Debug.DrawRay(position, direction1 * playerDistance1, Color.red);
            // check if the player's feet are in sight
            if (Physics.Raycast(position, direction1, playerDistance1, Helper.DefaultLayer))
            {
                Debug.DrawRay(position, direction1 * playerDistance1, Color.green);
                    
                var targetPosition2 = targetPosition1 + 2f * Vector3.up;
                var direction2 = targetPosition2 - position;
                var playerDistance2 = direction2.magnitude;
                direction2.Normalize();
                    
                // check if the player's head is in sight
                if (Physics.Raycast(position, direction2, playerDistance2, Helper.DefaultLayer))
                {
                    Debug.DrawRay(position, direction2 * playerDistance2, Color.green);
                    return false;
                }
            }
            return true;
        }
    }
}
