using F3PS;
using StarterAssets;
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

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        meleeAttackController.Init();
        longRangeAttackController.Init();
        SetActiveAttack(GameManager.Instance.PlayerData.ActiveAttack);
    }
    public void OnUpdate()
    {
        HandleSwitchAttack(_inputs.switchWeapon);
        HandleActiveAttack(_inputs.shoot, _inputs.look);
    }
    public void OnFixedUpdate()
    {
        _aimTargetPosition = crosshair.GetTargetPosition();
        switch (GameManager.Instance.PlayerData.ActiveAttack)
        {
            default:
                break;
        }
    }
    private void HandleActiveAttack(bool attack, Vector2 look)
    {
        switch (GameManager.Instance.PlayerData.ActiveAttack)
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
        var currentAttack = GameManager.Instance.PlayerData.ActiveAttack;
        var nextAttack = currentAttack == Attack.Melee ? Attack.LongRange : Attack.Melee;
        SetActiveAttack(nextAttack);
    }

    private void SetActiveAttack(Attack attack)
    {
        meleeAttackController.gameObject.SetActive(attack == Attack.Melee);
        longRangeAttackController.gameObject.SetActive(attack == Attack.LongRange);
        GameManager.Instance.PlayerData.ActiveAttack = attack;
    }
}
