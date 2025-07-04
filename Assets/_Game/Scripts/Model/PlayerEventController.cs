using System;

public class PlayerEventController
{
    public PlayerData Data { get; private set; }
    public PlayerEventController(PlayerData model)
    {
        Data = model;
    }

    #region Stamina

    public event Action OnStaminaUnlocked;
    public void UnlockStamina()
    {
        UnityEngine.Debug.Log("Stamina Unlocked");
        OnStaminaUnlocked?.Invoke();
    }

    public event Action<float> OnStaminaChanged;
    public void UpdateStamina(float currentStamina)
    {
        OnStaminaChanged?.Invoke(currentStamina);
        Data.CurrentStamina = currentStamina;
    }

    public event Action<float> OnMaxStaminaChanged;
    public void UpdateMaxStamina(float maxStamina)
    {
        OnMaxStaminaChanged?.Invoke(maxStamina);
        Data.MaxStamina = maxStamina;
    }

    public event Action<bool> OnIsRecoveringStaminaChanged;
    public void UpdateIsRecoveringStamina(bool isRecovering)
    {
        OnIsRecoveringStaminaChanged?.Invoke(isRecovering);
        Data.IsRecoveringStamina = isRecovering;
    }

    public event Action<bool> OnIsDepletingStaminaChanged;
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

    public event Action<Attack> OnAttackUnlocked;
    public void UnlockAttack(Attack attack)
    {
        if (Data.UnlockedAttacks.Contains(attack))
        {
            UnityEngine.Debug.LogError(attack + " is already unlocked.");
            return;
        }
        OnAttackUnlocked?.Invoke(attack);

        Data.UnlockedAttacks.Add(attack);
        if (Data.ActiveAttack == Attack.None)
        {
            SetActiveAttack(attack);
        }
        UnlockStamina();
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
        UnlockStamina();
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

    #region Abilities

    public event Action<Ability> OnAbilityUnlocked;
    public void UnlockAbility(Ability ability)
    {
        OnAbilityUnlocked?.Invoke(ability);
        if (Data.UnlockedAbilities.Contains(ability))
        {
            UnityEngine.Debug.LogError(ability + " is already unlocked.");
            return;
        }
        Data.UnlockedAbilities.Add(ability);
        UnlockStamina();
    }
    #endregion Abilities

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
