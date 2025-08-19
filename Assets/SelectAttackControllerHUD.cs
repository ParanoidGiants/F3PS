using F3PS;
using System;
using System.Linq;
using UnityEngine;

public class SelectAttackControllerHUD : MonoBehaviour
{
    private PlayerEventController PlayerEventController => GameManager.Instance.GameData.PlayerEventController;
    private PlayerData PlayerData => GameManager.Instance.GameData.PlayerData;

    public SelectableAttackHUD[] attackHuds;
    public SelectableAttackHUD[] activeAttackHuds;

    private void OnEnable()
    {
        PlayerEventController.OnActiveAttackChanged += SelectAttackHud;
        PlayerEventController.OnAttackUnlocked += UnlockHud;
    }

    private void OnDisable()
    {
        PlayerEventController.OnActiveAttackChanged -= SelectAttackHud;
        PlayerEventController.OnAttackUnlocked -= UnlockHud;
    }

    public void Start()
    {
        foreach (var attackHud in attackHuds)
        {
            attackHud.gameObject.SetActive(false);
        }

        foreach (var attack in PlayerData.UnlockedAttacks)
        {
            var attackHud = attackHuds.FirstOrDefault(x => x.attackType == attack);
            if (attackHud == null)
            {
                continue;
            }
            activeAttackHuds = activeAttackHuds.Append(attackHud).ToArray();
        }
        foreach (var skillHud in activeAttackHuds)
        {
            skillHud.gameObject.SetActive(true);
        }
    }

    public void SelectAttackHud(Attack attack)
    {
        foreach (var skillHud in attackHuds)
        {
            if (skillHud.attackType == attack)
            {
                skillHud.Select();
            }
            else
            {
                skillHud.Deselect();
            }
        }
    }

    private void UnlockHud(Attack attack)
    {
        var attackHud = Array.Find(attackHuds, x => x.attackType == attack);
        if (attackHud == null)
        {
            return;
        }
        attackHud.gameObject.SetActive(true);
        activeAttackHuds = activeAttackHuds.Append(attackHud).ToArray();
        attackHud.Deselect();
    }
}
