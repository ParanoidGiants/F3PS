using System;
using UnityEngine;

namespace Enemy.States
{
    [Serializable]
    public class Idle : State
    {
        private Transform _transform;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        public override void Initialize(BaseEnemy enemy, EnemyStateManager stateManager, string name)
        {
            base.Initialize(enemy, stateManager, name);
            _transform = enemy.transform;
            _originalPosition = _transform.position;
            _originalRotation = _transform.rotation;
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            enemy.navMeshAgent.isStopped = true;
        }

        public override void OnExit()
        {
            enemy.navMeshAgent.isStopped = false;
        }

        public override void Update()
        {
            _transform.rotation = Quaternion.Slerp(_transform.rotation, _originalRotation, Time.deltaTime);
            _transform.position = _originalPosition;
            
            AggressiveSwitchWhenTargetIsInSight();
        }
    }
}
