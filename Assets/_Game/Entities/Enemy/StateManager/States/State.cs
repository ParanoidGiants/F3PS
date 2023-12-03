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

        private void Awake()
        {
            _navMeshAgent = enemy.navMeshAgent;   
        }
        public virtual void OnEnter()
        {
            _navMeshAgent.speed = enemySpeed * enemy.TimeScale;
            _navMeshAgent.stoppingDistance = enemyStoppingDistance;
            UpdateSensorState();
            enemy.SetMaterial(material);
        }

        private void UpdateSensorState()
        {
            if (stateType == StateType.AGGRESSIVE)
            {
                stateManager.sensorController.SetState(SensorState.AGGRESSIVE);
            }
            else if (stateType is StateType.CHECKING or StateType.SUSPICIOUS or StateType.ENABLED_PHYSICS)
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
