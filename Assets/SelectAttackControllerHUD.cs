using F3PS;
using System;
using System.Linq;
using UnityEngine;

public class SelectAttackControllerHUD : MonoBehaviour
{
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;
    private PlayerData PlayerData => GameManager.Instance.PlayerData;

    public SelectableAttackHUD[] attackHuds;
    public SelectableAttackHUD[] activeAttackHuds;

    public void Start()
    {
        PlayerEventController.OnActiveAttackChanged += SelectAttackHud;
        PlayerEventController.OnAttackUnlocked += UnlockHud;

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
            Debug.LogWarning($"Attack HUD for {attack} not found.");
            return;
        }
        attackHud.gameObject.SetActive(true);
        activeAttackHuds = activeAttackHuds.Append(attackHud).ToArray();
        attackHud.Deselect();
    }
}
