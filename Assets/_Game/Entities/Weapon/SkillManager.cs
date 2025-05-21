using F3PS;
using StarterAssets;
using System;
using UnityEngine;

public enum Skill
{
    Telekinesis = 0,
    Rewind = 1,
    TimeBubble = 2,
}

public class SkillManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerSpace;
    public Crosshair crosshair;
        
    [Header("Skills")]
    public Skill activeSkill;
    public TelekinesisController telekinesisController;
    public RewindController rewindController;
    public TimeBubbleController timeBubbleController;

    private StarterAssetsInputs _inputs;        
    private Vector3 _aimTargetPosition;
    private bool _isWeaponSwitched;

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        crosshair.gameObject.SetActive(true);
        SetActiveSkill(activeSkill);
    }

    public void OnUpdate()
    {
        HandleSwitchSkill(_inputs.switchWeapon);
        HandleActiveSkill(
            _inputs.skill,
            _inputs.grab,
            _inputs.look,
            _inputs.telekinesisPushPull
        );
    }

    public void OnFixedUpdate()
    {
        _aimTargetPosition = crosshair.GetTargetPosition();

        switch (activeSkill)
        {
            case Skill.Telekinesis:
                telekinesisController.OnFixedUpdate();
                break;
            case Skill.Rewind:
                rewindController.OnFixedUpdate();
                break;
            case Skill.TimeBubble:
                break;
            default:
                break;
        }
    }

    private void HandleActiveSkill(bool skill, bool grab, Vector2 look, float telekinesisPushPull)
    {
        switch (activeSkill)
        {
            case Skill.Telekinesis:
                telekinesisController.OnUpdate(
                    skill,
                    grab,
                    look,
                    telekinesisPushPull
                );
                break;
            case Skill.Rewind:
                rewindController.OnUpdate(skill, grab, telekinesisPushPull);
                break;
            case Skill.TimeBubble:
                timeBubbleController.OnUpdate(skill, telekinesisPushPull, _aimTargetPosition);
                break;
            default:
                break;
        }
    }

    private void HandleSwitchSkill(bool switchWeapon)
    {
        if (!switchWeapon)
        {
            _isWeaponSwitched = false;
            return;
        }


        if (_isWeaponSwitched)
        {
            return;
        }

        _isWeaponSwitched = true;
        var nextSkill = (Skill)(((int)activeSkill + 1) % 3);
        activeSkill = nextSkill;
        SetActiveSkill(nextSkill);
    }

    private void SetActiveSkill(Skill nextSkill)
    {
        switch (nextSkill)
        {
            case Skill.Telekinesis:
                telekinesisController.gameObject.SetActive(true);
                rewindController.gameObject.SetActive(false);
                timeBubbleController.gameObject.SetActive(false);
                break;
            case Skill.Rewind:
                telekinesisController.gameObject.SetActive(false);
                rewindController.gameObject.SetActive(true);
                timeBubbleController.gameObject.SetActive(false);
                break;
            case Skill.TimeBubble:
                telekinesisController.gameObject.SetActive(false);
                rewindController.gameObject.SetActive(false);
                timeBubbleController.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
}