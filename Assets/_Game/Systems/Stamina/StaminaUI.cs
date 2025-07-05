using F3PS;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public PlayerData PlayerData => GameManager.Instance.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;
    public Image staminaBar;
    public Animator animator;
    
    void Awake()
    {
        PlayerEventController.OnStaminaChanged += UpdateStamina;
        PlayerEventController.OnIsRecoveringStaminaChanged += UpdateIsRecoveringStamina;
        PlayerEventController.OnIsDepletingStaminaChanged += UpdateIsDepletingStamina;
        PlayerEventController.OnStaminaUnlocked += ActivateStamina;

        if (!AnySkillIsUnlocked())
        {
            gameObject.SetActive(false);
        }
    }

    private bool AnySkillIsUnlocked()
    {
        return PlayerData.UnlockedAbilities.Any(skill => skill != Ability.None)
            || PlayerData.UnlockedSkills.Any(skill => skill != Skill.None)
            || PlayerData.UnlockedAttacks.Any(attack => attack != Attack.None);
    }

    private void ActivateStamina()
    {
        gameObject.SetActive(true);
    }

    private void UpdateIsRecoveringStamina(bool isRecovering)
    {
        animator.SetBool("recover", isRecovering);
    }

    private void UpdateIsDepletingStamina(bool isDepleting)
    {
        animator.SetBool("deplet", isDepleting);
    }

    private void UpdateStamina(float stamina)
    {
        staminaBar.fillAmount = stamina;
    }
}
