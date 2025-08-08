using System;

public class PlayerEventController
{
    public PlayerData Data { get; private set; }

    public void Initialize(PlayerData model)
    {
        Data = model;
        UpdateCurrentSpawnPoint(Data.CurrentSpawnPoint);
        UpdateStamina(Data.CurrentStamina);
        UpdateMaxStamina(Data.MaxStamina);
        UpdateIsRecoveringStamina(Data.IsRecoveringStamina);
        UpdateIsDepletingStamina(Data.IsDepletingStamina);
        SetActiveAttack(Data.ActiveAttack);
        SetActiveSkill(Data.ActiveSkill);
        SetAnubisScrollState(Data.AnubisScrollSkillData.State);
        SetAnubisScrollCurrentFrame(Data.AnubisScrollSkillData.CurrentFrame);
        SetAnubisScrollTotalFrames(Data.AnubisScrollSkillData.TotalFrames);
        SetAnubisScrollCurrentRecordingTime(Data.AnubisScrollSkillData.CurrentRecordingTime);
        SetKhonsuSphereTimeScale(Data.KhonsuSphereSkillData.TimeScale);
        SetKhonsuSphereActiveTime(Data.KhonsuSphereSkillData.ActiveTime);
        UpdateMaxHealth(Data.MaxHealth);
        UpdateCurrentHealth(Data.CurrentHealth);
        UpdateRotationSmoothTime(Data.RotationSmoothTime);
        UpdateRotationSpeedPitch(Data.RotationSpeedPitch);
        UpdateRotationSpeedYaw(Data.RotationSpeedYaw);
        UpdateMoveSpeed(Data.MoveSpeed);
        UpdateSprintSpeed(Data.SprintSpeed);
        UpdateSpeedChangeRate(Data.SpeedChangeRate);
        UpdateJumpHeight(Data.JumpHeight);
        UpdateDodgeHeight(Data.DodgeHeight);
        UpdateDodgeSpeed(Data.DodgeSpeed);
    }

    #region Progress
    public event Action<int> OnCurrentSpawnPointChanged;
    public void UpdateCurrentSpawnPoint(int currentSpawnPoint)
    {
        Data.CurrentSpawnPoint = currentSpawnPoint;
        OnCurrentSpawnPointChanged?.Invoke(currentSpawnPoint);
    }
    #endregion Progress

    #region Stamina

    public event Action OnStaminaUnlocked;
    public void UnlockStamina()
    {
        OnStaminaUnlocked?.Invoke();
    }

    public event Action<float> OnStaminaChanged;
    public void UpdateStamina(float currentStamina)
    {
        Data.CurrentStamina = currentStamina;
        OnStaminaChanged?.Invoke(currentStamina);
    }

    public event Action<float> OnMaxStaminaChanged;
    public void UpdateMaxStamina(float maxStamina)
    {
        Data.MaxStamina = maxStamina;
        OnMaxStaminaChanged?.Invoke(maxStamina);
    }

    public event Action<bool> OnIsRecoveringStaminaChanged;
    public void UpdateIsRecoveringStamina(bool isRecovering)
    {
        Data.IsRecoveringStamina = isRecovering;
        OnIsRecoveringStaminaChanged?.Invoke(isRecovering);
    }

    public event Action<bool> OnIsDepletingStaminaChanged;
    public void UpdateIsDepletingStamina(bool isDepleting)
    {
        Data.IsDepletingStamina = isDepleting;
        OnIsDepletingStaminaChanged?.Invoke(isDepleting);
    }
    #endregion Stamina

    #region Attack
    public event Action<Attack> OnActiveAttackChanged;
    public void SetActiveAttack(Attack attack)
    {
        Data.ActiveAttack = attack;
        OnActiveAttackChanged?.Invoke(attack);
    }

    public event Action<Attack> OnAttackUnlocked;
    public void UnlockAttack(Attack attack)
    {
        if (Data.UnlockedAttacks.Contains(attack))
        {
            UnityEngine.Debug.LogError(attack + " is already unlocked.");
            return;
        }

        Data.UnlockedAttacks.Add(attack);
        if (Data.ActiveAttack == Attack.None)
        {
            SetActiveAttack(attack);
        }
        UnlockStamina();
        OnAttackUnlocked?.Invoke(attack);
    }
    #endregion Attack

    #region Skills
    public event Action<Skill> OnActiveSkillChanged;
    public void SetActiveSkill(Skill skill)
    {
        Data.ActiveSkill = skill;
        OnActiveSkillChanged?.Invoke(skill);
    }

    public event Action<Skill> OnSkillUnlocked;
    public void UnlockSkill(Skill skill)
    {
        if (Data.UnlockedSkills.Contains(skill))
        {
            return;
        }

        Data.UnlockedSkills.Add(skill);
        if (Data.ActiveSkill == Skill.None)
        {
            SetActiveSkill(skill);
        }
        UnlockStamina();
        OnSkillUnlocked?.Invoke(skill);
    }

    #region KhonsuSphere
    public event Action<float> OnKhonsuSphereTimeScaleChanged;
    public void SetKhonsuSphereTimeScale(float timeScale)
    {
        Data.KhonsuSphereSkillData.TimeScale = timeScale;
        OnKhonsuSphereTimeScaleChanged?.Invoke(timeScale);
    }
    public event Action<float> OnKhonsuSphereActiveTimeChanged;
    public void SetKhonsuSphereActiveTime(float time)
    {
        Data.KhonsuSphereSkillData.ActiveTime = time;
        OnKhonsuSphereActiveTimeChanged?.Invoke(time);
    }
    #endregion KhonsuSphere
    
    #region AnubisScroll
    public event Action<AnubisScrollState> OnAnubisScrollStateChanged;
    public void SetAnubisScrollState(AnubisScrollState state)
    {
        Data.AnubisScrollSkillData.State = state;
        OnAnubisScrollStateChanged?.Invoke(state);
    }

    public event Action<int> OnAnubisScrollCurrentFrameChanged;
    public void SetAnubisScrollCurrentFrame(int frame)
    {
        Data.AnubisScrollSkillData.CurrentFrame = frame;
        OnAnubisScrollCurrentFrameChanged?.Invoke(frame);
    }
    public event Action<int> OnAnubisScrollTotalFramesChanged;
    public void SetAnubisScrollTotalFrames(int totalFrames)
    {
        Data.AnubisScrollSkillData.TotalFrames = totalFrames;
        OnAnubisScrollTotalFramesChanged?.Invoke(totalFrames);
    }

    public event Action<float> OnAnubisScrollCurrentRecordingTime;
    public void SetAnubisScrollCurrentRecordingTime(float time)
    {
        Data.AnubisScrollSkillData.CurrentRecordingTime = time;
        OnAnubisScrollCurrentRecordingTime?.Invoke(time);
    }

    #endregion AnubisScroll


    #endregion Skills

    #region Abilities

    public event Action<Ability> OnAbilityUnlocked;
    public void UnlockAbility(Ability ability)
    {
        if (Data.UnlockedAbilities.Contains(ability))
        {
            UnityEngine.Debug.LogError(ability + " is already unlocked.");
            return;
        }
        Data.UnlockedAbilities.Add(ability);
        UnlockStamina();
        OnAbilityUnlocked?.Invoke(ability);
    }
    #endregion Abilities

    #region Health
    public event Action<int> OnMaxHealthChanged;
    public void UpdateMaxHealth(int maxHealth)
    {
        Data.MaxHealth = maxHealth;
        OnMaxHealthChanged?.Invoke(maxHealth);
    }

    public event Action<int> OnCurrentHealthChanged;
    public void UpdateCurrentHealth(int currentHealth)
    {
        Data.CurrentHealth = currentHealth;
        OnCurrentHealthChanged?.Invoke(currentHealth);
    }
    #endregion Health

    #region Movement
    public event Action<float> OnRotationSmoothTimeChanged;
    public void UpdateRotationSmoothTime(float rotationSmoothTime)
    {
        Data.RotationSmoothTime = rotationSmoothTime;
        OnRotationSmoothTimeChanged?.Invoke(rotationSmoothTime);
    }

    public event Action<float> OnRotationSpeedPitchChanged;
    public void UpdateRotationSpeedPitch(float rotationSpeedPitch)
    {
        Data.RotationSpeedPitch = rotationSpeedPitch;
        OnRotationSpeedPitchChanged?.Invoke(rotationSpeedPitch);
    }

    public event Action<float> OnRotationSpeedYawChanged;
    public void UpdateRotationSpeedYaw(float rotationSpeedYaw)
    {
        Data.RotationSpeedYaw = rotationSpeedYaw;
        OnRotationSpeedYawChanged?.Invoke(rotationSpeedYaw);
    }

    public event Action<float> OnMoveSpeedChanged;
    public void UpdateMoveSpeed(float moveSpeed)
    {
        Data.MoveSpeed = moveSpeed;
        OnMoveSpeedChanged?.Invoke(moveSpeed);
    }

    public event Action<float> OnSprintSpeedChanged;
    public void UpdateSprintSpeed(float sprintSpeed)
    {
        Data.SprintSpeed = sprintSpeed;
        OnSprintSpeedChanged?.Invoke(sprintSpeed);
    }

    public event Action<float> OnSpeedChangeRateChanged;
    public void UpdateSpeedChangeRate(float speedChangeRate)
    {
        Data.SpeedChangeRate = speedChangeRate;
        OnSpeedChangeRateChanged?.Invoke(speedChangeRate);
    }

    public event Action<float> OnJumpHeightChanged;
    public void UpdateJumpHeight(float jumpHeight)
    {
        Data.JumpHeight = jumpHeight;
        OnJumpHeightChanged?.Invoke(jumpHeight);
    }

    public event Action<float> OnDodgeHeightChanged;
    public void UpdateDodgeHeight(float dodgeHeight)
    {
        Data.DodgeHeight = dodgeHeight;
        OnDodgeHeightChanged?.Invoke(dodgeHeight);
    }

    public event Action<float> OnDodgeSpeedChanged;
    public void UpdateDodgeSpeed(float dodgeSpeed)
    {
        Data.DodgeSpeed = dodgeSpeed;
        OnDodgeSpeedChanged?.Invoke(dodgeSpeed);
    }
    #endregion Movement
}
