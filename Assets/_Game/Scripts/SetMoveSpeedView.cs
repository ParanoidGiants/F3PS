using TMPro;
using UnityEngine.UI;

public class SetMoveSpeedView : SetModelValueView
{
    public Slider slider;
    public TextMeshProUGUI valueText;
    private float _moveSpeed;

    private void OnEnable()
    {
        PlayerEventController.OnMoveSpeedChanged += SetMoveSpeedSlider;
    }

    private void OnDisable()
    {
        PlayerEventController.OnMoveSpeedChanged -= SetMoveSpeedSlider;
    }

    private void Start()
    {
        slider.minValue = 1;
        slider.maxValue = 100;
        SetMoveSpeedSlider(PlayerData.MoveSpeed);
    }

    private void SetMoveSpeedSlider(float moveSpeed)
    {
        _moveSpeed = moveSpeed;
        UpdateText();
    }

    public void OnValueChanged(float value)
    {
        PlayerEventController.UpdateMoveSpeed((int)value);
    }

    private void UpdateText()
    {
        valueText.text = $"{_moveSpeed}";
    }
}
