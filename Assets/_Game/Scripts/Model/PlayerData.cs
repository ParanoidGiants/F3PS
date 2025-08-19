using System;
using System.Collections.Generic;
using UnityEngine;

public enum Ability
{
    None = 0,
    Glide = 1,
    Sprint = 2,
}

public enum Skill
{
    None = 0,
    KhonsuSphere = 1,
    ThotMind = 2,
    AnubisScroll = 3,
}

public enum Attack
{
    None = 0,
    HorusPalm = 1,
    OsirisKick = 2,
}

[Serializable]
public class KhonsuSphereSkillData
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

public enum AnubisScrollState
{
    None,
    Record,
    Playback,
    Paused,
    Rewind
}

[Serializable]
public class AnubisScrollSkillData
{
    [Header("Settings")]
    public float CoolDownTime = 0.5f;
    public float Duration = 2f;
    public float Speed = 1f;
    public float ScrollSpeed = 1f;
    public float MinimumDistance = 3f;
    public float MaximumDistance = 50f;
    public float FrameDuration = 0.1f;

    [Header("Watchers")]
    public AnubisScrollState State = AnubisScrollState.None;
    public int CurrentFrame = 0;
    public int TotalFrames = 0;
    public float CurrentRecordingTime = 0;
}

[Serializable]
public class ThotMindSkillData
{
    public float PushPullSpeed = 5.0f;
    public float RotateTimer = 1f;
    public float MoveSpeed = 5.0f;
    public float MinimumDistance = 3f;
    public float MaximumDistance = 50f;
    public float MaximumThrowSpeed = 3f;
}


[Serializable]
public class HorusPalmData
{
    public int Damage = 10;
    public float AttackSpeed = 100f;
    public float ImpactForceMultiplier = 1.0f;
    public float AttackCoolDownTimer = 0.2f;
    public float StaminaCost = 0.3f;
    public float ProjectileLifeDuration = 5f;
}

[Serializable]
public class OsirisKickData
{
    public int Damage = 10;
    public int NumberOfProjectiles = 8;
    public float StaminaCost = 10f;
    public float SpreadAngle = 45f;
    public float AttackSpeed = 100f;
    public float AttackCoolDownTimer = 0.2f;
    public float ProjectileLifeDuration = 5f;
}

[Serializable]
public class PlayerData
{
    [Space(10)]
    [Header("Move Settings")]
    public float MoveSpeed = 40;
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Header("Sprint Settings")]
    [Range(0f, 1f)]
    public float SprintDepletionRate = 0.05f;
    public float SprintSpeed = 5.335f;
    
    [Space(10)]
    [Header("Jump Settings")]
    public float JumpHeight = 8;
    public float JumpCoolDownTimer = 0.25f;

    [Space(10)]
    [Header("Dodge Settings")]
    public float DodgeHeight = 1.2f;
    public float DodgeSpeed = 60f;
    public float DodgeAscendTimer = 0.5f;
    public float DodgeLandTimer = 0.5f;
    public float DodgeCoolDownTimer = 0.25f;
    public float DodgeStaminaDepletionRate = 0.15f;

    [Space(10)]
    [Header("Rotation Settings")]   
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Range(0.0f, 1f)]
    public float RotationSpeedPitch = 0.2f;
    [Range(0.0f, 1f)]
    public float RotationSpeedYaw = 0.2f;
    public float AimSpeed = 2.0f;

    [Header("Progress")]
    public int CurrentSpawnPoint = 0;

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
    [Header("Ascende and Glide Settings")]
    public float AscendHeight = 1f;
    public float AscendDuration = 1f;
    public float GlideDuration = 1f;
    public float GlideDepletionRate = 0.05f;
    public float LandingDepth = 20f;


    [Space(20)]
    [Header("Attacks")]
    public List<Attack> UnlockedAttacks;
    public Attack ActiveAttack = Attack.None;
    [Header("Osiris Kick Settings")]
    public OsirisKickData OsirisKickData;
    [Header("Horus Palm Settings")]
    public HorusPalmData HorusPalmData;

    [Space(20)]
    [Header("Skills")]
    public List<Skill> UnlockedSkills;
    public Skill ActiveSkill = Skill.None;
    [Header("Khonsu Sphere Settings")]
    public KhonsuSphereSkillData KhonsuSphereSkillData;
    [Header("Anubis Scroll Settings")]
    public AnubisScrollSkillData AnubisScrollSkillData;
    [Header("Thot Mind Settings")]
    public ThotMindSkillData ThotMindSkillData;

    [Space(20)]
    [Header("Abilities")]
    public List<Ability> UnlockedAbilities;
}
