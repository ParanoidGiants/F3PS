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
            if (!HasTarget || TargetCandidates.Count < 2) return false;

            int targetsInSight = 0;
            var position = eyes.position;
            for (int i = 0; i < TargetCandidates.Count && targetsInSight < 2; i++)
            {
                var targetPosition = TargetCandidates[i].Center();
                var direction = targetPosition - position;
                var playerPartDistance = direction.magnitude;
                direction.Normalize();
                Debug.DrawRay(position, direction * playerPartDistance, Color.red);
                    
                // check if something is between the player and the eyes
                if (!Physics.Raycast(position, direction, playerPartDistance, Helper.DefaultLayer))
                {
                    targetsInSight++;
                }
            }
            return targetsInSight >= 2;
        }
    }
}
