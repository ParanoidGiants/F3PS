using System.Linq;
using UnityEngine;
using Enemy.States;

namespace Enemy
{
    public class EnemyStateManager : MonoBehaviour
    {
        public State currentState;
        public State[] states;

        private void Start()
        {
            currentState = GetState(StateType.IDLE);
            currentState.OnEnter();
        }

        void FixedUpdate()
        {
            currentState.OnUpdate();
        }
        
        public void SwitchState(StateType stateType)
        {
            currentState.OnExit();
            currentState = GetState(stateType);
            currentState.OnEnter();
        }

        private State GetState(StateType stateType)
        {
            return states.FirstOrDefault(x => x.stateType == stateType);
        }
    }
}
