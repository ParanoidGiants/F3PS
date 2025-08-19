using UnityEngine;
using UnityEngine.UI;
using F3PS;
using TMPro;

public class SetCurrentHealthView : MonoBehaviour
{
    private PlayerData PlayerData => GameManager.Instance.GameData.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.GameData.PlayerEventController;

    public Slider slider;
    public TextMeshProUGUI healthText;
    private bool initialized = false;

    private void OnEnable()
    {
        PlayerEventController.OnCurrentHealthChanged += SetCurrentHealthSlider;
        PlayerEventController.OnMaxHealthChanged += SetMaxHealth;

        slider.minValue = 1;
        slider.maxValue = PlayerData.MaxHealth;
        slider.value = PlayerData.CurrentHealth;

        initialized = true;
    }

    private void OnDisable()
    {
        PlayerEventController.OnCurrentHealthChanged -= SetCurrentHealthSlider;
        PlayerEventController.OnMaxHealthChanged -= SetMaxHealth;

        initialized = false;
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
