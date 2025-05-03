using F3PS;
using StarterAssets;
using UnityEngine;

public enum Skill
{
    Telekinesis = 0,
    Rewind = 1,
    TimeBubble = 2
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
        SwitchSkill(activeSkill);
    }

    public void OnUpdate()
    {
        HandleSwitchSkill();
        switch (activeSkill)
        {
            case Skill.Telekinesis:
                telekinesisController.OnUpdate(_inputs.shoot, _inputs.telekinesisPushPull);
                break;
            case Skill.Rewind:
                rewindController.OnUpdate(_inputs.shoot, _inputs.aimGrenade, _inputs.telekinesisPushPull);
                break;
            case Skill.TimeBubble:
                bool isAimingGrenade = _inputs.aimGrenade;
                timeBubbleController.OnUpdate(_inputs.shoot, _aimTargetPosition);
                break;
            default:
                break;
        }
    }

    private void HandleSwitchSkill()
    {
        if (!_inputs.switchWeapon)
        {
            _isWeaponSwitched = false;
            return;
        }


        if (!_isWeaponSwitched)
        {
            _isWeaponSwitched = true;
            var nextSkill = (Skill)(((int)activeSkill + 1) % 3);
            SwitchSkill(nextSkill);
        }
    }

    private void SwitchSkill(Skill nextSkill)
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
        activeSkill = nextSkill;
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
}