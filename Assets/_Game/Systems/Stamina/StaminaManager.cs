using TimeBending;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public float stamina;
    public float staminaMax = 100f;
    public float staminaRegenRate = 10f;
    public float staminaDepletionRate = 10f;
    public bool isDepleting;
    public bool isInRestMode;
    public float StaminaPercentage => stamina / staminaMax;

    private void Start()
    {
        stamina = staminaMax;
    }

    private void Update()
    {
        if (isInRestMode)
        {
            if (stamina < staminaMax)
            {
                stamina += staminaRegenRate * Time.unscaledDeltaTime;
            }
            else
            {
                stamina = staminaMax;
                isInRestMode = false;
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
        return !isInRestMode && stamina >= required;
    }

    private void EnterRestMode()
    {
        isDepleting = false;
        isInRestMode = true;
    }
}
