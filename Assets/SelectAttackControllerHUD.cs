using F3PS;
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
    }

    public void SelectAttackHud(Attack attack)
    {
        if (attack.Equals(Attack.None))
        {
            foreach (var skillHud in attackHuds)
            {
                skillHud.gameObject.SetActive(false);
            }
            return;
        }

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
}
