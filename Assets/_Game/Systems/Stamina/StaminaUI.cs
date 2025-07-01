using F3PS;
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
    }

    private void Start()
    {
        if (PlayerData.UnlockedAttacks.All(a => a.Equals(Attack.None))
            && PlayerData.UnlockedSkills.All(s => s.Equals(Skill.None))
            && PlayerData.UnlockedPassiveSkills.All(p => p.Equals(PassiveSkills.None))
        )
        {
            gameObject.SetActive(false);
        }
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
