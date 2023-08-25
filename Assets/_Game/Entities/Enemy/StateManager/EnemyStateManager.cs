using System.Linq;
using UnityEngine;
using F3PS.AI.Sensors;

namespace F3PS.AI.States
{
    public class EnemyStateManager : MonoBehaviour
    {
        [Header("Watchers")]
        [SerializeField]
        private State _currentState;

        [Space(10)]
        [Header("Settings")]
        public State[] states;
        public SensorController sensorController;
        private void Start()
        {
            _currentState = GetState(StateType.IDLE);
            _currentState.OnEnter();
        }

        void FixedUpdate()
        {
            _currentState.OnUpdate();
        }
        
        public void SwitchState(StateType stateType)
        {
            _currentState.OnExit();
            _currentState = GetState(stateType);
            _currentState.OnEnter();
        }

        private State GetState(StateType stateType)
        {
            return states.FirstOrDefault(x => x.stateType == stateType);
        }
    }
}
