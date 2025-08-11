using UnityEngine;

namespace F3PS.AI.Sensors
{
    public class VisionSensor : BaseSensor
    {
        public bool IsTargetInSight { get; private set; }
        public Transform eyes;

        private void FixedUpdate()
        {
            if (!HasTarget || TargetCandidates.Count < 2)
            {
                IsTargetInSight = false;
                return;
            }

            int targetsInSight = 0;
            var position = eyes.position;
            for (int i = 0; i < TargetCandidates.Count && targetsInSight < 2; i++)
            {
                var targetPosition = TargetCandidates[i].Center();
                var direction = targetPosition - position;
                var playerPartDistance = direction.magnitude;
                direction.Normalize();
                // check if something is between the player and the eyes
                Debug.DrawRay(position, direction * playerPartDistance, Color.green);
                if (!Physics.Raycast(position, direction, out var hit, playerPartDistance, Helper.DefaultLayer))
                {
                    targetsInSight++;
                }
                else
                {
                    Debug.DrawLine(position, hit.point, Color.red);
                }
            }
            IsTargetInSight = targetsInSight > 0;
        }
    }
}
