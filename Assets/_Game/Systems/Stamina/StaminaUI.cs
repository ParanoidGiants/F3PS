using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public Image staminaBar;
    public Animator animator;
    private StaminaManager _staminaManager;
    
    void Start()
    {
        _staminaManager = FindFirstObjectByType<StaminaManager>();
    }

    void Update()
    {
        staminaBar.fillAmount = _staminaManager.StaminaPercentage;
        animator.SetBool("isReloading", _staminaManager.isInRestMode);
        animator.SetBool("isUsing", _staminaManager.isDepleting);
    }
}
