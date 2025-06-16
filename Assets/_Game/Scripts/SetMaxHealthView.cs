using UnityEngine.UI;
using TMPro;

public class SetMaxHealthView: SetModelValueView
{
    public Slider slider;
    public TextMeshProUGUI healthText;
    private int _maxHealth;
    private bool initialized = false;

    private void OnEnable()
    {
        slider.minValue = 1;
        slider.maxValue = 3000;
        slider.value = PlayerData.MaxHealth;

        PlayerEventController.OnMaxHealthChanged += SetMaxHealthSlider;
        initialized = true;
    }

    private void OnDisable()
    {
        initialized = false;
        PlayerEventController.OnMaxHealthChanged -= SetMaxHealthSlider;
    }

    private void SetMaxHealthSlider(int maxHealth)
    {
        _maxHealth = maxHealth;
        UpdateText();
    }

    public void OnValueChanged(float value)
    {
        if (!initialized) return;

        if (PlayerData.CurrentHealth > value)
        {
            PlayerEventController.UpdateCurrentHealth((int)value);
        }
        PlayerEventController.UpdateMaxHealth((int)value);
    }

    private void UpdateText()
    {
        healthText.text = $"{_maxHealth}";
    }
}
