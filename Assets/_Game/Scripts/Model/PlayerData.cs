using System;
using UnityEngine;

public enum Skill
{
    Telekinesis = 0,
    Rewind = 1,
    TimeBubble = 2,
}

public enum Attack
{
    Melee = 0,
}

[Serializable]
public class PlayerData
{
    public void Init()
    {
        CurrentHealth = MaxHealth;
    }

    public int CurrentHealth;
    public int MaxHealth = 100;

    public Skill ActiveSkill = Skill.Telekinesis;
    public Attack ActiveAttack = Attack.Melee;

    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Range(0.0f, 1f)]
    public float RotationSpeedPitch = 0.2f;
    [Range(0.0f, 1f)]
    public float RotationSpeedYaw = 0.2f;
    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    public float SpeedChangeRate = 10.0f;
    public float JumpHeight = 1.2f;
    public float DodgeHeight = 1.2f;
    public float DodgeSpeed = 60f;

    public float SprintDepletionRate = 10f;
}
