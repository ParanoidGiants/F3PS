using F3PS;
using System;
using TimeBending;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public float stamina;
    public float staminaMax = 100f;
    public float staminaRegenRate = 10f;
    public float staminaDepletionRate = 10f;
    public bool isDepleting;
    public bool isRecovering;
    public float StaminaPercentage => stamina / staminaMax;

    private void Start()
    {
        stamina = staminaMax;
    }

    private void Update()
    {
        if (isRecovering)
        {
            if (stamina < staminaMax)
            {
                stamina += staminaRegenRate * Time.unscaledDeltaTime;
            }
            else
            {
                stamina = staminaMax;
                isRecovering = false;
            }
            return;
        }
        if (isDepleting)
        {
            isDepleting = false;
            return;
        }
        else
        {
            stamina += staminaRegenRate * Time.unscaledDeltaTime;
            stamina = Mathf.Clamp(stamina, 0f, staminaMax);
        }

    }

    public void Deplete(float deplete)
    {
        isDepleting = true;
        staminaDepletionRate -= deplete;
        if (staminaDepletionRate <= 0f)
        {
            staminaDepletionRate = 0f;
            EnterRestMode();
        }
    }

    public bool HasEnoughStamina(float required)
    {
        return !isRecovering && stamina >= required;
    }

    private void EnterRestMode()
    {
        isDepleting = false;
        isRecovering = true;
    }

    public bool Sprint()
    {
        if (isRecovering || !GameManager.Instance.inputs.sprint)
        {
            return false;
        }

        Deplete(GameManager.Instance.PlayerData.SprintDepletionRate * Time.deltaTime);
        return true;
    }
}
