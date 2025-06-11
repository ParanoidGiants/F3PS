using F3PS;
using StarterAssets;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerSpace;
    public Crosshair crosshair;
        
    [Header("Skills")]
    public TelekinesisController telekinesisController;
    public RewindController rewindController;
    public TimeBubbleController timeBubbleController;

    private StarterAssetsInputs _inputs;
    private Vector3 _aimTargetPosition;
    private bool _isSkillSwitched;

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        crosshair.gameObject.SetActive(true);
        SetActiveSkill(GameManager.Instance.PlayerData.ActiveSkill);
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

        switch (GameManager.Instance.PlayerData.ActiveSkill)
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
        switch (GameManager.Instance.PlayerData.ActiveSkill)
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
            _isSkillSwitched = false;
            return;
        }

        if (_isSkillSwitched)
        {
            return;
        }

        _isSkillSwitched = true;
        var activeSkill = GameManager.Instance.PlayerData.ActiveSkill;
        var nextSkill = (Skill)(((int)activeSkill + 1) % 3);
        SetActiveSkill(nextSkill);
    }

    private void SetActiveSkill(Skill nextSkill)
    {
        GameManager.Instance.PlayerData.ActiveSkill = nextSkill;
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

    public bool IsAiming()
    {
        switch (GameManager.Instance.PlayerData.ActiveSkill)
        {
            case Skill.Telekinesis:
                return telekinesisController.isMovingObjectThisFrame;
            case Skill.Rewind:
                return rewindController.IsAiming();
            case Skill.TimeBubble:
                return timeBubbleController.IsAiming();
            default:
                return false;
        }
    }
}