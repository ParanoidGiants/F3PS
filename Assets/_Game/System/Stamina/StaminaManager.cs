using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public float stamina;
    public float staminaMax = 100f;
    public float staminaRegenRate = 10f;
    public float staminaDepletionRateSprint = 10f;
    public float staminaDepletionRateAim = 0f;

    public bool _isSprinting = false;
    public bool _isAiming = false;
    public bool _isReloading = false;
    public float StaminaPercentage => stamina / staminaMax;

    private void Start()
    {
        stamina = staminaMax;
    }
    private void Update()
    {
        if (!_isReloading)
        {
            if (_isSprinting)
            {
                stamina -= staminaDepletionRateSprint * Time.deltaTime;
            }
            else if (_isAiming)
            {
                stamina -= staminaDepletionRateAim * Time.deltaTime;
            }
            else
            {
                stamina += staminaRegenRate * Time.deltaTime;
            }
        }
        else
        {
            stamina += staminaRegenRate * Time.deltaTime;
        }
        
        if (stamina >= staminaMax)
        {
            _isReloading = false;
        }
        else if (stamina < 0f)
        {
            _isReloading = true;
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
