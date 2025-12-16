using F3PS;
using System;
using System.Linq;
using UnityEngine;

public class SelectSkillControllerHUD : MonoBehaviour
{
    private PlayerEventController PlayerEventController => GameManager.Instance.saveGameManager.PlayerEventController;
    private PlayerData PlayerData => GameManager.Instance.GameData.PlayerData;

    public SelectableSkillHUD[] skillHuds;

    public SelectableSkillHUD[] activeSkillHuds;

    private void Start()
    {
        foreach (var skillHud in skillHuds)
        {
            skillHud.gameObject.SetActive(PlayerData.UnlockedSkills.Contains(skillHud.skillType));
        }
    } 

    private void OnEnable()
    {
        PlayerEventController.OnActiveSkillChanged += SelectSkillHud;
        PlayerEventController.OnSkillUnlocked += UnlockHud;
    }

    private void OnDisable()
    {
        PlayerEventController.OnActiveSkillChanged -= SelectSkillHud;
        PlayerEventController.OnSkillUnlocked -= UnlockHud;
    }

    private void Awake()
    {
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
        skillHud.gameObject.SetActive(true);
        activeSkillHuds = activeSkillHuds.Append(skillHud).ToArray();
        skillHud.Deselect();
    }
}
