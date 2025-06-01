using UnityEngine.UI;
using TMPro;

public class SetMaxHealthView: SetModelValueView
{
    public Slider slider;
    public TextMeshProUGUI healthText;
    private int _maxHealth;

    private void Start()
    {
        slider.minValue = 1;
        slider.maxValue = 3000;
        SetMaxHealthSlider(PlayerData.MaxHealth);
        PlayerEventController.OnMaxHealthChanged += SetMaxHealthSlider;
    }

    private void SetMaxHealthSlider(int maxHealth)
    {
        _maxHealth = maxHealth;
        UpdateText();
    }

    public void OnValueChanged(float value)
    {
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
