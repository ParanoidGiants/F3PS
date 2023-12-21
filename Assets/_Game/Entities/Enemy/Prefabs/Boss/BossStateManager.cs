using System.Collections;
using System.Collections.Generic;
using F3PS.AI.States;
using UnityEngine;

public class BossStateManager : EnemyStateManager
{
    override
    public void Initialize()
    {
        foreach (var state in states)
        {
            state.Initialize();
        }
        _currentState = GetState(StateType.AGGRESSIVE);
        _currentState.OnEnter();
    }
    
    override
    public void SwitchState(StateType stateType) { }
}
