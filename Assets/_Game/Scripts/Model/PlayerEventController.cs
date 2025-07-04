using System;
using System.Runtime.CompilerServices;

public class PlayerEventController
{
    public PlayerData Data { get; private set; }
    public PlayerEventController(PlayerData model)
    {
        Data = model;
    }

    #region Stamina
    public event Action<float> OnStaminaChanged;
    public event Action<float> OnMaxStaminaChanged;
    public event Action<float> OnStaminaRecoveryRateChanged;
    public event Action<bool> OnIsRecoveringStaminaChanged;
    public event Action<bool> OnIsDepletingStaminaChanged;

    public void UpdateCurrentStamina(float currentStamina)
    {
        OnStaminaChanged?.Invoke(currentStamina);
        Data.CurrentStamina = currentStamina;
    }

    public void UpdateMaxStamina(float maxStamina)
    {
        OnMaxStaminaChanged?.Invoke(maxStamina);
        Data.MaxStamina = maxStamina;
    }

    public void UpdateStaminaRecoveryRate(float staminaRecoveryRate)
    {
        OnStaminaRecoveryRateChanged?.Invoke(staminaRecoveryRate);
        Data.StaminaRecoveryRate = staminaRecoveryRate;
    }

    public void UpdateIsRecoveringStamina(bool isRecovering)
    {
        OnIsRecoveringStaminaChanged?.Invoke(isRecovering);
        Data.IsRecoveringStamina = isRecovering;
    }

    public void UpdateIsDepletingStamina(bool isDepleting)
    {
        OnIsDepletingStaminaChanged?.Invoke(isDepleting);
        Data.IsDepletingStamina = isDepleting;
    }
    #endregion Stamina

    #region Attack
    public event Action<Attack> OnActiveAttackChanged;
    public void SetActiveAttack(Attack attack)
    {
        OnActiveAttackChanged?.Invoke(attack);
        Data.ActiveAttack = attack;
    }
    #endregion Attack

    #region Skills
    public event Action<Skill> OnActiveSkillChanged;
    public void SetActiveSkill(Skill skill)
    {
        OnActiveSkillChanged?.Invoke(skill);
        Data.ActiveSkill = skill;
    }

    public event Action<Skill> OnSkillUnlocked;
    public void UnlockSkill(Skill skill)
    {
        if (skill == Skill.None)
        {
            UnityEngine.Debug.LogError("Cannot unlock Skill.None.");
            return;
        }

        if (Data.UnlockedSkills.Contains(skill))
        {
            UnityEngine.Debug.LogError(skill + " is already unlocked.");
            return;
        }
        OnSkillUnlocked?.Invoke(skill);

        Data.UnlockedSkills.Add(skill);
        if (Data.ActiveSkill == Skill.None)
        {
            SetActiveSkill(skill);
        }
    }

    #region KhonsuSphere
    public event Action<float> OnKhonsuSphereTimeScaleChanged;
    public void SetKhonsuSphereTimeScale(float timeScale)
    {
        OnKhonsuSphereTimeScaleChanged?.Invoke(timeScale);
        Data.KhonsuSphereSkillData.TimeScale = timeScale;
    }
    public event Action<float> OnKhonsuSphereActiveTimeChanged;
    public void SetKhonsuSphereActiveTime(float time)
    {
        OnKhonsuSphereActiveTimeChanged?.Invoke(time);
        Data.KhonsuSphereSkillData.ActiveTime = time;
    }
    #endregion KhonsuSphere
    
    #region AnubisScroll
    public event Action<AnubisScrollState> OnAnubisScrollStateChanged;
    public void SetAnubisScrollState(AnubisScrollState state)
    {
        OnAnubisScrollStateChanged?.Invoke(state);
        Data.AnubisScrollSkillData.State = state;
    }

    public event Action<int> OnAnubisScrollCurrentFrameChanged;
    public void SetAnubisScrollCurrentFrame(int frame)
    {
        OnAnubisScrollCurrentFrameChanged?.Invoke(frame);
        Data.AnubisScrollSkillData.CurrentFrame = frame;
    }
    public event Action<int> OnAnubisScrollTotalFramesChanged;
    public void SetAnubisScrollTotalFrames(int totalFrames)
    {
        OnAnubisScrollTotalFramesChanged?.Invoke(totalFrames);
        Data.AnubisScrollSkillData.TotalFrames = totalFrames;
    }

    public event Action<float> OnAnubisScrollCurrentRecordingTime;
    public void SetAnubisScrollCurrentRecordingTime(float time)
    {
        OnAnubisScrollCurrentRecordingTime?.Invoke(time);
        Data.AnubisScrollSkillData.CurrentRecordingTime = time;
    }

    #endregion AnubisScroll

    #endregion Skills

    #region Health
    public event Action<int> OnMaxHealthChanged;
    public event Action<int> OnCurrentHealthChanged;

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
    #endregion Health

    #region Movement
    public event Action<int> OnRotationSmoothTimeChanged;
    public event Action<float> OnRotationSpeedPitchChanged;
    public event Action<float> OnRotationSpeedYawChanged;
    public event Action<float> OnMoveSpeedChanged;
    public event Action<float> OnSprintSpeedChanged;
    public event Action<float> OnSpeedChangeRateChanged;
    public event Action<float> OnJumpHeightChanged;
    public event Action<float> OnDodgeHeightChanged;
    public event Action<float> OnDodgeSpeedChanged;
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
    #endregion Movement
}
