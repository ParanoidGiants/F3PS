using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image healthBar;
    private Animator _animator;
    
    void Start()
    {
        healthBar.fillAmount = 1f;
        _animator = GetComponent<Animator>();
    }

    public void UpdateHealth(float healthPercentage)
    {
        healthBar.fillAmount = healthPercentage;
        _animator.SetBool("hit", true);
    }
}
