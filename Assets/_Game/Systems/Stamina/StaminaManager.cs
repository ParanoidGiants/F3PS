using TimeBending;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public float stamina;
    public float staminaMax = 100f;
    public float staminaRegenRate = 10f;
    public float staminaDepletionRateSprint = 10f;
    public float staminaDepletionRateAim = 0f;
    public float staminaDepletionRateSlowMo = 20f;

    public bool _isSprinting = false;
    public bool _isAiming = false;
    public bool _isRegenerating = false;
    public bool _isSlowMotion = false;
    public float StaminaPercentage => stamina / staminaMax;

    public TimeManager timeManager;

    private void Start()
    {
        stamina = staminaMax;
        if (timeManager == null)
        {
            timeManager = FindObjectOfType<TimeManager>();
        }
    }
    private void Update()
    {
        _isSlowMotion = timeManager.isActive;

        if (!_isRegenerating)
        {
            if (!_isSlowMotion && !_isSprinting && !_isAiming)
            {
                stamina += staminaRegenRate * Time.unscaledDeltaTime;
            }
            else
            {
                if (_isSlowMotion)
                {
                    stamina -= staminaDepletionRateSlowMo * Time.unscaledDeltaTime;
                }
                if (_isSprinting)
                {
                    stamina -= staminaDepletionRateSprint * Time.unscaledDeltaTime;
                }
                if (_isAiming)
                {
                    stamina -= staminaDepletionRateAim * Time.unscaledDeltaTime;
                }
            }
        }
        else
        {
            stamina += staminaRegenRate * Time.unscaledDeltaTime;
        }
        
        if (stamina >= staminaMax)
        {
            _isRegenerating = false;
        }
        else if (stamina < 0f)
        {
            _isRegenerating = true;
            timeManager.StopSlowMotion();
        }
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }


    public void UpdateSprinting(bool inputSprint)
    {
        _isSprinting = inputSprint;
    }

    public void UpdateAiming(bool inputAim)
    {
        _isAiming = inputAim;
    }
}
