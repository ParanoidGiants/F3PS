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
    private int _currentHealth;
    private int _maxHealth;

    private void Start()
    {
        slider.minValue = 1;
        slider.maxValue = 3000;
        SetCurrentHealthSlider(PlayerData.CurrentHealth);
        PlayerEventController.OnCurrentHealthChanged += SetCurrentHealthSlider;
    }

    private void SetCurrentHealthSlider(int currentHealth)
    {
        _currentHealth = currentHealth;
        slider.value = currentHealth;
        UpdateText();
    }

    public void OnValueChanged(float value)
    {
        PlayerEventController.UpdateCurrentHealth((int)value);
    }

    private void UpdateText()
    {
        healthText.text = $"{_currentHealth}/{_maxHealth}";
    }
}
