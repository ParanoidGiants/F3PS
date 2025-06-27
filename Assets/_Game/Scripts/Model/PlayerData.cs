using System;
using UnityEngine;

public enum Skill
{
    Telekinesis = 1,
    Rewind = 2,
    TimeBubble = 0,
}

public enum Attack
{
    Melee = 0,
    LongRange = 1,
}

[Serializable]
public class PlayerData
{
    public void Init()
    {
        CurrentHealth = MaxHealth;
    }

    [Space(10)]
    [Header("Health")]
    public int CurrentHealth;
    public int MaxHealth = 100;

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
    public float SprintSpeed = 5.335f;
    public float SprintDepletionRate = 10f;
    public float SpeedChangeRate = 10.0f;

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

    [Space(10)]
    [Header("Skills and Attacks")]
    public Skill ActiveSkill = Skill.Telekinesis;
    public Attack ActiveAttack = Attack.Melee;
}
