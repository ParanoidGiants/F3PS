using F3PS;
using StarterAssets;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerSpace;
    public Crosshair crosshair;
    [Header("Attacks")]
    public MeleeAttackController meleeAttackController;
    private StarterAssetsInputs _inputs;
    private Vector3 _aimTargetPosition;
    private bool _isAttackSwitched;

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        SetActiveAttack(GameManager.Instance.PlayerData.ActiveAttack);
        meleeAttackController.Init(playerSpace);
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
                var targetPosition = _aimTargetPosition;
                meleeAttackController.OnUpdate(attack, targetPosition);
                break;
            default:
                break;
        }
    }
    private void HandleSwitchAttack(bool switchWeapon)
    {
        if (switchWeapon && !_isAttackSwitched)
        {
            _isAttackSwitched = true;
            SetActiveAttack(Attack.Melee);
        }
        else if (!switchWeapon)
        {
            _isAttackSwitched = false;
        }
    }

    private void SetActiveAttack(Attack attack)
    {
        meleeAttackController.gameObject.SetActive(attack == Attack.Melee);
        GameManager.Instance.PlayerData.ActiveAttack = attack;
    }
}
