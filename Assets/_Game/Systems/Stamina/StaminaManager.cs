using F3PS;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public PlayerData playerData => GameManager.Instance.GameData.PlayerData;
    public PlayerEventController playerEventController => GameManager.Instance.GameData.PlayerEventController;

    public bool isDepleting = false;

    public bool IsRecoveringStamina => playerData.IsRecoveringStamina;

    private void Update()
    {
        if (playerData.IsRecoveringStamina)
        {
            if (playerData.CurrentStamina <= playerData.MaxStamina)
            {
                var stamina = playerData.CurrentStamina + playerData.StaminaRecoveryRate * Time.unscaledDeltaTime;
                playerEventController.UpdateStamina(stamina);
            }
            else
            {
                Debug.Log("Stamina is full, stopping recovery.");
                playerEventController.UpdateStamina(playerData.MaxStamina);
                playerEventController.UpdateIsRecoveringStamina(false);
            }
        }
        else if (!playerData.IsDepletingStamina && !isDepleting)
        {
            var stamina = playerData.CurrentStamina + playerData.StaminaRecoveryRate * Time.unscaledDeltaTime;
            stamina = Mathf.Clamp(stamina, 0f, playerData.MaxStamina);
            playerEventController.UpdateStamina(stamina);
        }
        else if (isDepleting)
        {
            isDepleting = false;
        }
        else
        {
            playerEventController.UpdateIsDepletingStamina(false);
        }
    }

    public void Deplete(float deplete)
    {
        var stamina = playerData.CurrentStamina - deplete;
        isDepleting = true;

        playerEventController.UpdateIsDepletingStamina(true);
        playerEventController.UpdateStamina(stamina);

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
