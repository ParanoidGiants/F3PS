using System.Linq;
using UnityEngine;
using Enemy.States;

namespace Enemy
{
    public class EnemyStateManager : MonoBehaviour
    {
        [Header("Watchers")]
        [SerializeField]
        private State _currentState;

        [Space(10)]
        [Header("Watchers")]
        public State[] states;
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
