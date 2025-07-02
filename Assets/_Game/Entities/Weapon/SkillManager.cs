using F3PS;
using StarterAssets;
using System;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private PlayerData PlayerData => GameManager.Instance.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;

    [Header("References")]
    public Transform playerSpace;
    public Crosshair crosshair;
        
    [Header("Skills")]
    public TelekinesisController telekinesisController;
    public RewindController rewindController;
    public KhonsuSphereController khonsuSphereController;

    private StarterAssetsInputs _inputs;
    private Vector3 _aimTargetPosition;
    private bool _isSkillSwitched;

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        crosshair.gameObject.SetActive(true);
        SetActiveSkill(PlayerData.ActiveSkill);

        PlayerEventController.OnActiveSkillChanged += SetActiveSkill;
    }

    public void OnUpdate()
    {
        HandleSwitchSkill(_inputs.switchSkill);
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

        switch (PlayerData.ActiveSkill)
        {
            case Skill.Telekinesis:
                telekinesisController.OnFixedUpdate();
                break;
            case Skill.Rewind:
                rewindController.OnFixedUpdate();
                break;
            default:
                break;
        }
    }

    private void HandleActiveSkill(bool skill, bool grab, Vector2 look, float telekinesisPushPull)
    {
        switch (PlayerData.ActiveSkill)
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
            case Skill.KhonsuSphere:
                khonsuSphereController.OnUpdate(skill, telekinesisPushPull, _aimTargetPosition);
                break;
            default:
                break;
        }
    }

    private void HandleSwitchSkill(bool switchWeapon)
    {
        if (!AreAnyTwoSkillsUnlocked())
        {
            return;
        }

        if (!switchWeapon)
        {
            _isSkillSwitched = false;
            return;
        }

        if (_isSkillSwitched)
        {
            return;
        }

        _isSkillSwitched = true;
        var activeSkillIndex = PlayerData.UnlockedSkills.IndexOf(PlayerData.ActiveSkill);
        var nextSkillIndex = ((activeSkillIndex + 1) % PlayerData.UnlockedSkills.Count);
        var nextSkill = PlayerData.UnlockedSkills[nextSkillIndex];
        PlayerEventController.SetActiveSkill(nextSkill);
        PlayerData.ActiveSkill = nextSkill;
        SetActiveSkill(nextSkill);
    }

    private bool AreAnyTwoSkillsUnlocked()
    {
        return PlayerData.UnlockedSkills.Count(s => s != Skill.None) > 1;
    }

    private void SetActiveSkill(Skill nextSkill)
    {
        switch (nextSkill)
        {
            case Skill.Telekinesis:
                telekinesisController.gameObject.SetActive(true);
                rewindController.gameObject.SetActive(false);
                khonsuSphereController.gameObject.SetActive(false);
                break;
            case Skill.Rewind:
                telekinesisController.gameObject.SetActive(false);
                rewindController.gameObject.SetActive(true);
                khonsuSphereController.gameObject.SetActive(false);
                break;
            case Skill.KhonsuSphere:
                telekinesisController.gameObject.SetActive(false);
                rewindController.gameObject.SetActive(false);
                khonsuSphereController.gameObject.SetActive(true);
                break;
            default:
                telekinesisController.gameObject.SetActive(false);
                rewindController.gameObject.SetActive(false);
                khonsuSphereController.gameObject.SetActive(false);
                break;
        }
    }

    public bool IsAiming()
    {
        switch (GameManager.Instance.PlayerData.ActiveSkill)
        {
            case Skill.Telekinesis:
                return telekinesisController.isMovingObjectThisFrame;
            case Skill.Rewind:
                return rewindController.IsAiming();
            case Skill.KhonsuSphere:
                return khonsuSphereController.IsAiming();
            default:
                return false;
        }
    }

    internal void OnLateUpdate()
    {

        switch (GameManager.Instance.PlayerData.ActiveSkill)
        {
            case Skill.Telekinesis:
                telekinesisController.OnLateUpdate();
                break;

            case Skill.Rewind:
                rewindController.OnLateUpdate();
                break;

            default:
                break;
        }
    }
}