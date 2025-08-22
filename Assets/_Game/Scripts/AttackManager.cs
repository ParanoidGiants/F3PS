using F3PS;
using StarterAssets;
using System.Linq;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    private PlayerData PlayerData => GameManager.Instance.GameData.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.saveGameManager.PlayerEventController;

    [Header("References")]
    public Crosshair crosshair;

    [Header("Attacks")]
    public OsirisKickController osirisKickController;
    public HorusPalmController horusPalmController;

    private StarterAssetsInputs _inputs;
    private Vector3 _aimTargetPosition;
    private bool _isAttackSwitched;

    private Attack ActiveAttack => GameManager.Instance.GameData.PlayerData.ActiveAttack;

    private void OnEnable()
    {
        PlayerEventController.OnActiveAttackChanged += SetActiveAttack;
    }

    private void OnDisable()
    {
        PlayerEventController.OnActiveAttackChanged -= SetActiveAttack;
    }

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        osirisKickController.Init();
        horusPalmController.Init();
        // SetActiveAttack(ActiveAttack);
        // PlayerEventController.SetActiveAttack(ActiveAttack);
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
            case Attack.OsirisKick:
                osirisKickController.OnUpdate(attack, targetPosition: _aimTargetPosition);
                break;
            case Attack.HorusPalm:
                horusPalmController.OnUpdate(attack, targetPosition: _aimTargetPosition);
                break;
            default:
                break;
        }
    }
    private void HandleSwitchAttack(bool switchWeapon)
    {
        if (!PlayerData.UnlockedAttacks.Contains(Attack.OsirisKick)
            || !PlayerData.UnlockedAttacks.Contains(Attack.HorusPalm))
        {
            return;
        }
        if (ActiveAttack == Attack.OsirisKick && osirisKickController.isAttacking
            || ActiveAttack == Attack.HorusPalm && horusPalmController.isAttacking)
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
        var nextAttack = currentAttack == Attack.OsirisKick ? Attack.HorusPalm : Attack.OsirisKick;
        PlayerEventController.SetActiveAttack(nextAttack);
    }

    private void SetActiveAttack(Attack attack)
    {
        osirisKickController.gameObject.SetActive(attack == Attack.OsirisKick);
        horusPalmController.gameObject.SetActive(attack == Attack.HorusPalm);
    }
}
