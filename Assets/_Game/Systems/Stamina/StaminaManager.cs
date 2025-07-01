using F3PS;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public PlayerData playerData => GameManager.Instance.PlayerData;
    public PlayerEventController playerEventController => GameManager.Instance.PlayerEventController;

    public bool IsRecoveringStamina => playerData.IsRecoveringStamina;

    private void Awake()
    {
        playerEventController.UpdateCurrentStamina(playerData.MaxStamina);
    }

    private void Update()
    {
        if (playerData.IsRecoveringStamina)
        {
            if (playerData.CurrentStamina <= playerData.MaxStamina)
            {
                var stamina = playerData.CurrentStamina + playerData.StaminaRecoveryRate * Time.unscaledDeltaTime;
                playerEventController.UpdateCurrentStamina(stamina);
            }
            else
            {
                Debug.Log("Stamina is full, stopping recovery.");
                playerEventController.UpdateCurrentStamina(playerData.MaxStamina);
                playerEventController.UpdateIsRecoveringStamina(false);
            }
        }
        else if (playerData.IsDepletingStamina)
        {
            playerEventController.UpdateIsDepletingStamina(false);
        }
        else
        {
            var stamina = playerData.CurrentStamina + playerData.StaminaRecoveryRate * Time.unscaledDeltaTime;
            stamina = Mathf.Clamp(stamina, 0f, playerData.MaxStamina);
            playerEventController.UpdateCurrentStamina(stamina);
        }
    }

    public void Deplete(float deplete)
    {
        playerEventController.UpdateIsDepletingStamina(true);
        var stamina = playerData.CurrentStamina - deplete;
        playerEventController.UpdateCurrentStamina(stamina);

        if (stamina <= 0f)
        {
            stamina = 0f;
            EnterRestMode();
        }
    }

    private void EnterRestMode()
    {
        playerEventController.UpdateIsDepletingStamina(false);
        playerEventController.UpdateIsRecoveringStamina(true);
    }
}
