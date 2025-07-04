using F3PS;
using System;
using System.Linq;
using UnityEngine;

public class SelectSkillControllerHUD : MonoBehaviour
{
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;
    private PlayerData PlayerData => GameManager.Instance.PlayerData;

    public SelectableSkillHUD[] skillHuds;

    public SelectableSkillHUD[] activeSkillHuds;

    private void Awake()
    {
        PlayerEventController.OnActiveSkillChanged += SelectSkillHud;
        PlayerEventController.OnSkillUnlocked += UnlockHud;

        foreach (var skillHud in skillHuds)
        {
            skillHud.gameObject.SetActive(false);
        }

        foreach (var skill in PlayerData.UnlockedSkills)
        {
            var skillHud = skillHuds.FirstOrDefault(x => x.skillType == skill);
            if (skillHud == null)
            {
                continue;
            }
            activeSkillHuds = activeSkillHuds.Append(skillHud).ToArray();
        }
        foreach (var skillHud in activeSkillHuds)
        {
            skillHud.gameObject.SetActive(true);
        }
    }


    public void SelectSkillHud(Skill skill)
    {
        foreach (var skillHud in activeSkillHuds)
        {
            if (skillHud.skillType == skill)
            {
                skillHud.Select();
            }
            else
            {
                skillHud.Deselect();
            }
        }
    }

    private void UnlockHud(Skill skill)
    {
        var skillHud = skillHuds.FirstOrDefault(x => x.skillType == skill);
        if (skillHud == null)
        {
            Debug.LogError("Skill HUD does not exists for " + skill);
            return;
        }
        skillHud.gameObject.SetActive(true);
        activeSkillHuds = activeSkillHuds.Append(skillHud).ToArray();
        skillHud.Deselect();
    }
}
