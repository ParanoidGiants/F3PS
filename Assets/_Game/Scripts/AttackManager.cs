using F3PS;
using StarterAssets;
using System;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [Header("References")]
    public Crosshair crosshair;
    [Header("Attacks")]
    public MeleeAttackController meleeAttackController;
    public LongRangeAttackController longRangeAttackController;

    private StarterAssetsInputs _inputs;
    private Vector3 _aimTargetPosition;
    private bool _isAttackSwitched;

    private Attack ActiveAttack => GameManager.Instance.PlayerData.ActiveAttack;

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        meleeAttackController.Init();
        longRangeAttackController.Init();
        SetActiveAttack(ActiveAttack);
    }
    public void OnUpdate()
    {
        HandleSwitchAttack(_inputs.switchWeapon);
        HandleActiveAttack(_inputs.shoot, _inputs.look);
    }
    public void OnFixedUpdate()
    {
        _aimTargetPosition = crosshair.GetTargetPosition();
        switch (ActiveAttack)
        {
            default:
                break;
        }
    }
    private void HandleActiveAttack(bool attack, Vector2 look)
    {
        switch (ActiveAttack)
        {
            case Attack.Melee:
                meleeAttackController.OnUpdate(attack, targetPosition: _aimTargetPosition);
                break;
            case Attack.LongRange:
                longRangeAttackController.OnUpdate(attack, targetPosition: _aimTargetPosition);
                break;
            default:
                break;
        }
    }
    private void HandleSwitchAttack(bool switchWeapon)
    {
        if (!CanSwitchCurrentAttack())
        {
            return;
        }

        if (!switchWeapon)
        {
            _isAttackSwitched = false;
            return;
        }

        if (_isAttackSwitched)
        {
            return;
        }

        _isAttackSwitched = true;
        var currentAttack = ActiveAttack;
        var nextAttack = currentAttack == Attack.Melee ? Attack.LongRange : Attack.Melee;
        SetActiveAttack(nextAttack);
    }

    private bool CanSwitchCurrentAttack()
    {
        return ActiveAttack == Attack.Melee && !meleeAttackController.isAttacking
            || ActiveAttack == Attack.LongRange && !longRangeAttackController.isAttacking;
    }

    private void SetActiveAttack(Attack attack)
    {
        meleeAttackController.gameObject.SetActive(attack == Attack.Melee);
        longRangeAttackController.gameObject.SetActive(attack == Attack.LongRange);
        GameManager.Instance.PlayerData.ActiveAttack = attack;
    }
}
