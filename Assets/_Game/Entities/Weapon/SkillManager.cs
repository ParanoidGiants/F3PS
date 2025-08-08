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
    public ThotMindController thotMindController;
    public AnubisScrollController anubisScrollController;
    public KhonsuSphereController khonsuSphereController;

    private StarterAssetsInputs _inputs;
    private Vector3 _aimTargetPosition;
    private bool _isSkillSwitched;

    private void OnEnable()
    {
        PlayerEventController.OnActiveSkillChanged += SetActiveSkill;
    }

    private void OnDisable()
    {
        PlayerEventController.OnActiveSkillChanged -= SetActiveSkill;
    }

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        crosshair.gameObject.SetActive(true);
        // SetActiveSkill(PlayerData.ActiveSkill);
        // PlayerEventController.SetActiveSkill(PlayerData.ActiveSkill);
    }

    public void OnUpdate()
    {
        HandleSwitchSkill(_inputs.switchSkill);
        HandleActiveSkill(
            _inputs.skill,
            _inputs.grab,
            _inputs.look,
            _inputs.pushPull
        );
    }

    public void OnFixedUpdate()
    {

        _aimTargetPosition = crosshair.GetTargetPosition();

        switch (PlayerData.ActiveSkill)
        {
            case Skill.ThotMind:
                thotMindController.OnFixedUpdate();
                break;
            case Skill.AnubisScroll:
                anubisScrollController.OnFixedUpdate();
                break;
            default:
                break;
        }

        anubisScrollController.OnFixedUpdateForCurrentCandidate();
    }

    private void HandleActiveSkill(bool skill, bool grab, Vector2 look, float thotMindPushPull)
    {
        switch (PlayerData.ActiveSkill)
        {
            case Skill.ThotMind:
                thotMindController.OnUpdate(
                    skill,
                    grab,
                    look,
                    thotMindPushPull
                );
                break;
            case Skill.AnubisScroll:
                anubisScrollController.OnUpdate(skill, grab, thotMindPushPull);
                break;
            case Skill.KhonsuSphere:
                khonsuSphereController.OnUpdate(skill, thotMindPushPull, _aimTargetPosition);
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
            case Skill.ThotMind:
                thotMindController.gameObject.SetActive(true);
                anubisScrollController.gameObject.SetActive(false);
                khonsuSphereController.gameObject.SetActive(false);
                break;
            case Skill.AnubisScroll:
                thotMindController.gameObject.SetActive(false);
                anubisScrollController.gameObject.SetActive(true);
                khonsuSphereController.gameObject.SetActive(false);
                break;
            case Skill.KhonsuSphere:
                thotMindController.gameObject.SetActive(false);
                anubisScrollController.gameObject.SetActive(false);
                khonsuSphereController.gameObject.SetActive(true);
                break;
            default:
                thotMindController.gameObject.SetActive(false);
                anubisScrollController.gameObject.SetActive(false);
                khonsuSphereController.gameObject.SetActive(false);
                break;
        }
    }

    public bool IsAiming()
    {
        switch (GameManager.Instance.PlayerData.ActiveSkill)
        {
            case Skill.ThotMind:
                return thotMindController.isMovingObjectThisFrame;
            case Skill.AnubisScroll:
                return anubisScrollController.IsAiming();
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
            case Skill.ThotMind:
                thotMindController.OnLateUpdate();
                break;

            case Skill.AnubisScroll:
                anubisScrollController.OnLateUpdate();
                break;

            default:
                break;
        }
    }
}