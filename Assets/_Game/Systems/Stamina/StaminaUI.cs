using F3PS;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public PlayerData PlayerData => GameManager.Instance.GameData.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.GameData.PlayerEventController;
    public Image staminaBar;
    public Animator animator;

    private void OnEnable()
    {
        PlayerEventController.OnStaminaChanged += UpdateStamina;
        PlayerEventController.OnIsRecoveringStaminaChanged += UpdateIsRecoveringStamina;
        PlayerEventController.OnIsDepletingStaminaChanged += UpdateIsDepletingStamina;
        PlayerEventController.OnStaminaUnlocked += ActivateStamina;
    }

    private void OnDisable()
    {
        PlayerEventController.OnStaminaChanged -= UpdateStamina;
        PlayerEventController.OnIsRecoveringStaminaChanged -= UpdateIsRecoveringStamina;
        PlayerEventController.OnIsDepletingStaminaChanged -= UpdateIsDepletingStamina;
        PlayerEventController.OnStaminaUnlocked -= ActivateStamina;
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
