using System.Linq;
using UnityEngine;
using F3PS.AI.Sensors;
using System;

namespace F3PS.AI.States
{
    public class EnemyStateManager : MonoBehaviour
    {
        [Header("Watchers")]
        [SerializeField]
        protected State _currentState;

        [Space(10)]
        [Header("Settings")]
        public State[] states;
        public SensorController sensorController;
        public virtual void Initialize()
        {
            foreach (var state in states)
            {
                state.Initialize();
            }
            
            _currentState = GetState(StateType.IDLE);
            _currentState.OnEnter();
        }

        public void OnPhysicsUpdate()
        {
            _currentState.OnPhysicsUpdate();
        }

        public void OnFrameUpdate()
        {
            _currentState.OnFrameUpdate();
        }
        
        public virtual void SwitchState(StateType stateType)
        {
            if (_currentState.stateType == StateType.DYING)
            {
                Debug.LogWarning("Enemy is dying. State change is not possible");
                return;
            }

            _currentState.OnExit();
            _currentState = GetState(stateType);
            _currentState.OnEnter();
        }

        protected State GetState(StateType stateType)
        {
            return states.FirstOrDefault(x => x.stateType == stateType);
        }

        public bool IsAggressive()
        {
            return _currentState.stateType is StateType.AGGRESSIVE;
        }

        private bool IsAttacking()
        {
            return _currentState.stateType is StateType.AGGRESSIVE && (_currentState as Aggressive).IsAttacking;
        }

        public void Hit()
        {
            if (!IsAttacking())
            {
                SwitchState(StateType.HIT);
            }
        }

        public bool IsDying()
        {
            return _currentState.stateType is StateType.DYING;
        }
    }
}
