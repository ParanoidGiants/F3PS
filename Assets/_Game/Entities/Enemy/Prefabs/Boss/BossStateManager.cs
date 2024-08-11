using F3PS.AI.States;
using F3PS.AI.States.Action;
using F3PS.Enemy;

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
        BossEnemy boss = (BossEnemy)_currentState.enemy;
        if (stateType is StateType.AGGRESSIVE)
        {
            boss.EnableHealthUI();
        }
        else if (_currentState.stateType is StateType.AGGRESSIVE)
        {
            boss.DisableHealthUI();
        }
        base.SwitchState(stateType);
    }
    
    public void SwitchAttack(AttackType attackType)
    {
        if (_currentState is not Aggressive aggressive) return;
        
        aggressive.ChangeAttack(attackType);
    }
}
