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



    private void OnEnable()
    {
        foreach (var instruction in GetComponentsInChildren<InstructionElementController>())
        {
            instruction.gameObject.SetActive(false);
        }

        if (GameManager.Instance.saveGameManager.GameData.PlayerData.CurrentSpawnPoint == 0)
        {
            ShowMovementInstruction();
        }

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
        instructionOnMove.SetupInstructionToFollow("Use the left stick to move");
        instructionOnMove.OnInstructionFollowed = () =>
        {
            ShowSprintingInstruction();
        };
    }

    public void ShowSprintingInstruction()
    {
        instructionOnSprint.gameObject.SetActive(true);
        instructionOnSprint.SetupInstructionToFollow("Hold ZL to run faster");
        instructionOnSprint.OnInstructionFollowed = () =>
        {
            ShowDodgeInstruction();
        };
    }

    public void ShowDodgeInstruction()
    {
        instructionOnDodge.gameObject.SetActive(true);
        instructionOnDodge.SetupInstructionToFollow("Press Y to dodge roll");
        instructionOnDodge.OnInstructionFollowed = () =>
        {
            ShowJumpInstruction();
        };
    }

    public void ShowJumpInstruction()
    {
        instructionOnJump.gameObject.SetActive(true);
        instructionOnJump.SetupInstructionToFollow("Press B to jump");
    }

    public void ShowShootInstruction(Attack attack)
    {
        if (attack != Attack.HorusPalm)
        {
            return;
        }

        instructionOnShootHorusPalm.gameObject.SetActive(true);
        instructionOnShootHorusPalm.SetupInstructionToFollow("Press ZR to use the Horus Palm");
    }
    

    private void ShowThrowKhonsuSphereInstruction(Skill skill)
    {
        if (skill != Skill.KhonsuSphere)
        {
            return;
        }
        instructionOnThrowKhonsuSphere.gameObject.SetActive(true);
        instructionOnThrowKhonsuSphere.SetupInstructionToFollow("Press R to throw the Khonsu Sphere");
    }
}
