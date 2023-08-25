using F3PS.AI.Sensors;
using F3PS.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace F3PS.AI.States
{
    public class State : MonoBehaviour
    {
        [Header("Base References")]
        public NavMeshAgent navMeshAgent;
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
            navMeshAgent.speed = enemySpeed;
            navMeshAgent.stoppingDistance = enemyStoppingDistance;
            SetSensorState();
            enemy.SetMaterial(material);
        }

        private void SetSensorState()
        {
            if (stateType == StateType.IDLE)
            {
                stateManager.sensorController.SetState(SensorState.IDLE);
            }
            else if (stateType == StateType.AGGRESSIVE)
            {
                stateManager.sensorController.SetState(SensorState.AGGRESSIVE);
            }
            else
            {
                stateManager.sensorController.SetState(SensorState.SEARCHING);
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
