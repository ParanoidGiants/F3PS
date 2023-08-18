using System;
using UnityEngine;

namespace Enemy
{
    public class Vision : MonoBehaviour
    {
        public Transform eyes;
        public Hittable target;
        public int triggerCount;

        public bool canTargetBeDetected;
        private void OnTriggerEnter(Collider other)
        {
            var hittable = other.GetComponent<Hittable>();
            if (!hittable || !Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;

            triggerCount++;
            
            if (triggerCount > 1) return;
            canTargetBeDetected = true;
            target = hittable;
        }

        private void OnTriggerExit(Collider other)
        {
            var hittable = other.GetComponent<Hittable>();
            if (!hittable || !Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;

            triggerCount--;
            
            if (triggerCount > 0) return;
            canTargetBeDetected = false;
            target = null;
        }
        
            
        public bool IsTargetInSight()
        {
            if (!canTargetBeDetected) return false;
            
            var position = eyes.position;
            var targetPosition1 = target.Center();
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

        private void OnDisable()
        {
            triggerCount = 0;
            canTargetBeDetected = false;
            target = null;
        }
    }
}
