using F3PS;
using UnityEngine;

public class SelectAttackControllerHUD : MonoBehaviour
{
    public SelectableAttackHUD[] skillHuds;

    public void Start()
    {
        GameManager.Instance.PlayerEventController.OnActiveAttackChanged += SelectSkillHud;
    }

    public void SelectSkillHud(Attack attack)
    {
        if (attack.Equals(Attack.None))
        {
            foreach (var skillHud in skillHuds)
            {
                skillHud.gameObject.SetActive(false);
            }
            return;
        }

        foreach (var skillHud in skillHuds)
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
}
