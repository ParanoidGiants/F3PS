using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class Vision : MonoBehaviour
    {
        public Transform eyes;
        public Hittable SelectedTarget { get; private set; }
        public int triggerCount;
        public List<Hittable> targetCandidates;

        public bool canTargetBeDetected;
        private void OnTriggerEnter(Collider other)
        {
            var hittable = other.GetComponent<Hittable>();
            if (!hittable || !Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;

            triggerCount++;
            targetCandidates.Add(hittable);
            EvaluateBestTarget();
        }

        private void OnTriggerExit(Collider other)
        {
            var hittable = other.GetComponent<Hittable>();
            if (!hittable || !Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;

            triggerCount--;
            targetCandidates.Remove(hittable);
            EvaluateBestTarget();
        }
        
            
        public bool IsTargetInSight()
        {
            if (!canTargetBeDetected) return false;
            
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
        

        private void EvaluateBestTarget()
        {
            if (targetCandidates.Count == 0)
            {
                canTargetBeDetected = false;
                SelectedTarget = null;
            }
            else
            {
                canTargetBeDetected = true;
                SelectedTarget = targetCandidates[0];
                foreach (var targetCandidate in targetCandidates)
                {
                    if (targetCandidate.damageMultiplier > SelectedTarget.damageMultiplier)
                    {
                        SelectedTarget = targetCandidate;
                    }
                }
            }
        }

        private void OnDisable()
        {
            triggerCount = 0;
            canTargetBeDetected = false;
            SelectedTarget = null;
        }
    }
}
