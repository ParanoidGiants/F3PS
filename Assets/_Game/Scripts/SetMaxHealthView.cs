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
        PlayerEventController.OnMaxHealthChanged += SetMaxHealthSlider;

        slider.minValue = 1;
        slider.maxValue = 3000;
        slider.value = PlayerData.MaxHealth;

        initialized = true;
    }

    private void OnDisable()
    {
        PlayerEventController.OnMaxHealthChanged -= SetMaxHealthSlider;
        initialized = false;
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
