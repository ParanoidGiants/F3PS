using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public Image staminaBar;
    public Animator animator;
    private StaminaManager _staminaManager;
    
    void Start()
    {
        _staminaManager = FindObjectOfType<StaminaManager>();
    }

    void Update()
    {
        staminaBar.fillAmount = _staminaManager.StaminaPercentage;
        animator.SetBool("isReloading", _staminaManager._isRegenerating);
        animator.SetBool("isUsing", _staminaManager._isAiming || _staminaManager._isSprinting);
    }
}
