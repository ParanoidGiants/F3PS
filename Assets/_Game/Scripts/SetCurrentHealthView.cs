using UnityEngine;
using UnityEngine.UI;
using F3PS;
using TMPro;

public class SetCurrentHealthView : MonoBehaviour
{
    private PlayerData PlayerData => GameManager.Instance.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;

    public Slider slider;
    public TextMeshProUGUI healthText;
    private bool initialized = false;

    private void OnEnable()
    {
        slider.minValue = 1;
        slider.maxValue = PlayerData.MaxHealth;
        slider.value = PlayerData.CurrentHealth;

        PlayerEventController.OnCurrentHealthChanged += SetCurrentHealthSlider;
        PlayerEventController.OnMaxHealthChanged += SetMaxHealth;
        initialized = true;
    }

    private void OnDisable()
    {
        initialized = false;
        PlayerEventController.OnCurrentHealthChanged -= SetCurrentHealthSlider;
        PlayerEventController.OnMaxHealthChanged -= SetMaxHealth;
    }

    private void SetCurrentHealthSlider(int currentHealth)
    {
        slider.maxValue = PlayerData.MaxHealth;
        slider.value = currentHealth;
        healthText.text = $"{PlayerData.CurrentHealth}/{PlayerData.MaxHealth}";
    }

    private void SetMaxHealth(int maxHealth)
    {
        SetCurrentHealthSlider(PlayerData.CurrentHealth);
    }

    public void OnValueChanged(float value)
    {
        if (!initialized) return;

        PlayerEventController.UpdateCurrentHealth((int)value);
    }
}
