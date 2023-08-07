using Enemy.States;
using UnityEngine;

namespace Enemy
{
    public class EnemyStateManager : MonoBehaviour
    {
        public string currentStateName;
        private State currentState;

        [Space(20)]
        [Header("References")]
        public BaseEnemy enemy;
        
        [Space(20)]
        [Header("States")]
        public Idle idle;
        public Checking checking;
        public Aggressive aggressive;
        public Rush rush;
        public Suspicious suspicious;
        public ReturnToIdle returnToIdle;

        [Space(20)]
        [Header("Watchers")]
        public bool isTargetInSight;
        public Transform target;
        public int triggerCount;
        
        public void Start()
        {
            idle.Initialize(enemy, this, "Idle");
            checking.Initialize(enemy, this, "Checking");
            aggressive.Initialize(enemy, this, "Aggressive");
            rush.Initialize(enemy, this, "Rush");
            suspicious.Initialize(enemy, this, "Suspicious");
            returnToIdle.Initialize(enemy, this, "ReturnToIdle");
            SwitchState(idle);
        }

        void FixedUpdate()
        {
            isTargetInSight = IsTargetInSight();
            currentState.Update();
        }
        
        public void SwitchState(State newState)
        {
            if (currentState != null)
            {
                currentState.OnExit();
            }
            currentState = newState;
            currentState.OnEnter();
            
            currentStateName = currentState.Name;
        }
        
        public bool IsTargetInSight()
        {
            if (triggerCount == 0) return false;
            
            var position = enemy.headMeshRenderer.transform.position;
            
            var targetPosition1 = target.position;
            var direction1 = targetPosition1 - position;
            var playerDistance1 = direction1.magnitude;
            
            direction1.Normalize();
            Debug.DrawRay(position, direction1 * playerDistance1, Color.red);
            // check if the player's feet are in sight
            if (Physics.Raycast(position, direction1, playerDistance1, Helper.DefaultLayer))
            {
                Debug.DrawRay(position, direction1 * playerDistance1, Color.green);
                
                var targetPosition2 = targetPosition1 + Vector3.up;
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

        private void OnTriggerEnter(Collider other)
        {
            if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
            triggerCount++;
            target = other.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
            triggerCount--;
            
            if (triggerCount > 0) return;
            target = null;
        }
    }
}
