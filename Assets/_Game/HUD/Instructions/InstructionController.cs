using System;
using F3PS;
using UnityEngine;

public class InstructionController : MonoBehaviour
{
    public InstructionElementController instructionOnMove;
    public InstructionElementController instructionOnSprint;
    public InstructionElementController instructionOnDodge;
    public InstructionElementController instructionOnJump;
    public InstructionElementController instructionOnShootHorusPalm;
    public InstructionElementController instructionOnThrowKhonsuSphere;

    private void Start()
    {
        foreach (var instruction in GetComponentsInChildren<InstructionElementController>())
        {
            instruction.gameObject.SetActive(false);
        }

        if (GameManager.Instance.saveGameManager.GameData.PlayerData.CurrentSpawnPoint == 0)
        {
            ShowMovementInstruction();
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.saveGameManager.PlayerEventController.OnAttackUnlocked += ShowShootInstruction;
        GameManager.Instance.saveGameManager.PlayerEventController.OnSkillUnlocked += ShowThrowKhonsuSphereInstruction;
    }

    private void OnDisable()
    {
        GameManager.Instance.saveGameManager.PlayerEventController.OnAttackUnlocked -= ShowShootInstruction;
        GameManager.Instance.saveGameManager.PlayerEventController.OnSkillUnlocked -= ShowThrowKhonsuSphereInstruction;
    }

    public void ShowMovementInstruction()
    {
        instructionOnMove.gameObject.SetActive(true);
        instructionOnMove.ShowInstructionToFollow();
        instructionOnMove.OnInstructionFollowed = () =>
        {
            ShowSprintingInstruction();
        };
    }

    public void ShowSprintingInstruction()
    {
        instructionOnSprint.gameObject.SetActive(true);
        instructionOnSprint.ShowInstructionToFollow();
        instructionOnSprint.OnInstructionFollowed = () =>
        {
            ShowDodgeInstruction();
        };
    }

    public void ShowDodgeInstruction()
    {
        instructionOnDodge.gameObject.SetActive(true);
        instructionOnDodge.ShowInstructionToFollow();
        instructionOnDodge.OnInstructionFollowed = () =>
        {
            ShowJumpInstruction();
        };
    }

    public void ShowJumpInstruction()
    {
        instructionOnJump.gameObject.SetActive(true);
        instructionOnJump.ShowInstructionToFollow();
    }

    public void ShowShootInstruction(Attack attack)
    {
        if (attack != Attack.HorusPalm)
        {
            return;
        }

        instructionOnShootHorusPalm.gameObject.SetActive(true);
        instructionOnShootHorusPalm.ShowInstructionToFollow();
    }
    

    private void ShowThrowKhonsuSphereInstruction(Skill skill)
    {
        if (skill != Skill.KhonsuSphere)
        {
            return;
        }
        instructionOnThrowKhonsuSphere.gameObject.SetActive(true);
        instructionOnThrowKhonsuSphere.ShowInstructionToFollow();
    }
}
