using System;

public class PlayerEventController
{
    public PlayerData Data { get; private set; }

    public event Action<int> OnMaxHealthChanged;
    public event Action<int> OnCurrentHealthChanged;
    public event Action<int> OnRotationSmoothTimeChanged;
    public event Action<float> OnRotationSpeedPitchChanged;
    public event Action<float> OnRotationSpeedYawChanged;
    public event Action<float> OnMoveSpeedChanged;
    public event Action<float> OnSprintSpeedChanged;
    public event Action<float> OnSpeedChangeRateChanged;
    public event Action<float> OnJumpHeightChanged;
    public event Action<float> OnDodgeHeightChanged;
    public event Action<float> OnDodgeSpeedChanged;
    public PlayerEventController(PlayerData model)
    {
        Data = model;
        Data.Init();
    }

    public void UpdateMaxHealth(int maxHealth)
    {
        OnMaxHealthChanged?.Invoke(maxHealth);
        Data.MaxHealth = maxHealth;
    }

    public void UpdateCurrentHealth(int currentHealth)
    {
        OnCurrentHealthChanged?.Invoke(currentHealth);
        Data.CurrentHealth = currentHealth;
    }

    public void UpdateRotationSmoothTime(int rotationSmoothTime)
    {
        OnRotationSmoothTimeChanged?.Invoke(rotationSmoothTime);
        Data.RotationSmoothTime = rotationSmoothTime;
    }
    public void UpdateRotationSpeedPitch(float rotationSpeedPitch)
    {
        OnRotationSpeedPitchChanged?.Invoke(rotationSpeedPitch);
        Data.RotationSpeedPitch = rotationSpeedPitch;
    }
    public void UpdateRotationSpeedYaw(float rotationSpeedYaw)
    {
        OnRotationSpeedYawChanged?.Invoke(rotationSpeedYaw);
        Data.RotationSpeedYaw = rotationSpeedYaw;
    }
    public void UpdateMoveSpeed(float moveSpeed)
    {
        OnMoveSpeedChanged?.Invoke(moveSpeed);
        Data.MoveSpeed = moveSpeed;
    }
    public void UpdateSprintSpeed(float sprintSpeed)
    {
        OnSprintSpeedChanged?.Invoke(sprintSpeed);
        Data.SprintSpeed = sprintSpeed;
    }
    public void UpdateSpeedChangeRate(float speedChangeRate)
    {
        OnSpeedChangeRateChanged?.Invoke(speedChangeRate);
        Data.SpeedChangeRate = speedChangeRate;
    }
    public void UpdateJumpHeight(float jumpHeight)
    {
        OnJumpHeightChanged?.Invoke(jumpHeight);
        Data.JumpHeight = jumpHeight;
    }
    public void UpdateDodgeHeight(float dodgeHeight)
    {
        OnDodgeHeightChanged?.Invoke(dodgeHeight);
        Data.DodgeHeight = dodgeHeight;
    }
    public void UpdateDodgeSpeed(float dodgeSpeed)
    {
        OnDodgeSpeedChanged?.Invoke(dodgeSpeed);
        Data.DodgeSpeed = dodgeSpeed;
    }
}
