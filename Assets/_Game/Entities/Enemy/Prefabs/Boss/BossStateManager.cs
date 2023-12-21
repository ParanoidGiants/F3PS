using System.Collections;
using System.Collections.Generic;
using F3PS.AI.States;
using F3PS.Enemy;
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
        _currentState = GetState(StateType.IDLE);
        _currentState.OnEnter();
    }

    override
    public void SwitchState(StateType stateType)
    {
        base.SwitchState(stateType);
        if (stateType is StateType.AGGRESSIVE)
        {
            BossEnemy boss = (BossEnemy)_currentState.enemy;
            boss.EnableHealthUI();
        }
    }
}
