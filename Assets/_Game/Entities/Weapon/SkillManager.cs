using F3PS;
using StarterAssets;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerSpace;
    public Crosshair crosshair;
        
    [Header("Weapons")]
    public TelekinesisController telekinesisController;
    public ThrowTimeBubbleGrenade grenade;

    private StarterAssetsInputs _inputs;        
    private Vector3 _aimTargetPosition;

    public void Init()
    {
        _inputs = GameManager.Instance.inputs;
        crosshair.gameObject.SetActive(true);
    }

    public void OnUpdate()
    {
        bool isAimingGrenade = _inputs.aimGrenade;

        if (grenade.HandleThrow(isAimingGrenade, _aimTargetPosition))
        {
            return;
        }

        bool isShooting = _inputs.shoot;
        float telekinesisPushPull = _inputs.telekinesisPushPull;
        telekinesisController.OnUpdate(isShooting, telekinesisPushPull);
    }

    public void OnFixedUpdate()
    {
        _aimTargetPosition = crosshair.GetTargetPosition();
        telekinesisController.OnFixedUpdate();
    }
}