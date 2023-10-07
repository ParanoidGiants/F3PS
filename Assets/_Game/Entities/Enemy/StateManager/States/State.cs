using System;
using F3PS.AI.Sensors;
using F3PS.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace F3PS.AI.States
{
    public class State : MonoBehaviour
    {
        [Header("Base References")]
        protected NavMeshAgent _navMeshAgent;
        public BaseEnemy enemy;
        public EnemyStateManager stateManager;
        
        [Space(10)]
        [Header("Base Settings")]
        public StateType stateType;
        public float enemySpeed;
        public float enemyStoppingDistance;
        public Material material;

        public virtual void OnEnter()
        {
            if (_navMeshAgent == null)
            {
                _navMeshAgent = enemy.navMeshAgent;
            }
            _navMeshAgent.speed = enemySpeed;
            _navMeshAgent.stoppingDistance = enemyStoppingDistance;
            SetSensorState();
            enemy.SetMaterial(material);
        }

        private void SetSensorState()
        {
            if (stateType == StateType.AGGRESSIVE)
            {
                stateManager.sensorController.SetState(SensorState.AGGRESSIVE);
            }
            else if (stateType is StateType.CHECKING or StateType.SUSPICIOUS)
            {
                stateManager.sensorController.SetState(SensorState.SEARCHING);
            }
            else
            {
                stateManager.sensorController.SetState(SensorState.IDLE);
            }
        }

        public virtual void OnUpdate()
        {
            if (stateType != StateType.AGGRESSIVE && stateManager.sensorController.IsTargetDetected())
            {
                stateManager.SwitchState(StateType.AGGRESSIVE);
            }
        }
        public virtual void OnExit() {}
    }
}
