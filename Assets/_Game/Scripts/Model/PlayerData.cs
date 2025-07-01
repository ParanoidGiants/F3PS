using System;
using System.Collections.Generic;
using UnityEngine;

public enum PassiveSkills
{
    None = 0,
    Glide = 1,
    Sprint = 2,
}

public enum Skill
{
    None = 0,
    TimeBubble = 1,
    Telekinesis = 2,
    Rewind = 3,
}

public enum Attack
{
    None = 0,
    Melee = 1,
    LongRange = 2,
}

[Serializable]
public class TimeBubbleSkillData
{
    [Header("Settings")]
    public float TargetSize = 10f;
    public float ActiveDuration = 5f;
    public float ThrowPower = 1f;
    public float ChangeTimeScaleSpeed = 1f;

    [Header("Watchers")]
    public bool IsEnabled = false;
    public Vector3 Position = Vector3.zero;
    public float TimeScale = 1f;
    public float ActiveTime = 0f;
}

public class TelekinesisSkillData
{
    public float PushPullForce = 10f;
    public float MaxDistance = 10f;
    public float CoolDownTime = 0.5f;
}

public class RewindSkillData
{
    public float CoolDownTime = 0.5f;
    public float Duration = 2f;
    public float Speed = 1f;
}


[Serializable]
public class LongRangeAttackData
{
    public float AttackSpeed = 100f;
    public float ImpactForceMultiplier = 1.0f;
    public float AttackCoolDownTimer = 0.2f;
    public float StaminaCost = 0.3f;
}

[Serializable]
public class MeleeAttackData
{
    public int NumberOfProjectiles = 8;
    public float StaminaCost = 10f;
    public float SpreadAngle = 45f;
    public float AttackSpeed = 100f;
    public float AttackCoolDownTimer = 0.2f;
}

[Serializable]
public class PlayerData
{
    [Space(10)]
    [Header("Health")]
    public int CurrentHealth;
    public int MaxHealth = 100;

    [Space(10)]
    [Header("Stamina")]
    [Range(0f, 1f)]
    public float CurrentStamina;
    [Range(0f, 1f)]
    public float MaxStamina = 1f;
    [Range(0f, 1f)]
    public float StaminaRecoveryRate = 5;
    public bool IsRecoveringStamina = false;
    public bool IsDepletingStamina = false;

    [Space(10)]
    [Header("Sprint Settings")]
    [Range(0f, 1f)]
    public float SprintDepletionRate = 0.05f;
    public float SprintSpeed = 5.335f;
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Header("Move Settings")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Range(0.0f, 1f)]
    public float RotationSpeedPitch = 0.2f;
    [Range(0.0f, 1f)]
    public float RotationSpeedYaw = 0.2f;
    public float MoveSpeed = 2.0f;
    public float AimSpeed = 2.0f;

    [Space(10)]
    [Header("Jump Settings")]
    public float JumpCoolDownTimer = 0.25f;
    public float JumpHeight = 1.2f;
    public float LandingDepth = 20f;
    public float AscendDuration = 1f;
    public float GlideDuration = 1f;

    [Space(10)]
    [Header("Dodge Settings")]
    public float DodgeHeight = 1.2f;
    public float DodgeSpeed = 60f;
    public float DodgeAscendTimer = 0.5f;
    public float DodgeLandTimer = 0.5f;
    public float DodgeCoolDownTimer = 0.25f;

    [Space(20)]
    [Header("Attacks")]
    public List<Attack> UnlockedAttacks;
    public Attack ActiveAttack = Attack.None;
    [Header("Melee Attack Settings")]
    public MeleeAttackData MeleeAttackData;
    [Header("Long Range Attack Settings")]
    public LongRangeAttackData LongRangeAttackData;

    [Space(20)]
    [Header("Skills")]
    public List<Skill> UnlockedSkills;
    public Skill ActiveSkill = Skill.None;
    [Header("Time Bubble Skill Settings")]
    public TimeBubbleSkillData TimeBubbleSkillData;

    [Space(20)]
    [Header("Passive Skills")]
    public List<PassiveSkills> UnlockedPassiveSkills;

}
