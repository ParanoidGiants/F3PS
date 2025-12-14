using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InputActionType
{
    Press,
    Hold,
    Move
}

public enum GamePadInput
{
    LeftStick,
    RightStick,
    DigitalNorth,
    DigitalSouth,
    DigitalWest,
    DigitalEast,
    ActionButtonNorth,
    ActionButtonSouth,
    ActionButtonWest,
    ActionButtonEast,
    ShoulderTriggerLeft,
    ShoulderTriggerRight,
    ShoulderButtonLeft,
    ShoulderButtonRight,
    StartButton,
    SelectButton
}

public class InstructionElementController : MonoBehaviour
{
    [Header("Settings")]
    public InputActionType inputAction;
    public GamePadInput input;
    public string resultingAction;

    [Header("UI Elements")]
    public TextMeshProUGUI inputActionText;
    public TextMeshProUGUI resultingActionText;
    
    [Header("Animators")]
    public Animator gamePadInputAnimator;
    public Animator animator;
    public Action OnInstructionFollowed;

    private void OnEnable()
    {
        animator.SetTrigger("Show");
        inputActionText.text = inputAction.ToString();
        resultingActionText.text = resultingAction;
    }

    public void ShowInstructionToFollow()
    {
        string animatorTrigger = $"{inputAction}_{input}";
        Debug.Log($"Showing instruction: {animatorTrigger}");
        gamePadInputAnimator.SetTrigger(animatorTrigger);
    }

    public void ProcessFollowedInstruction()
    {
        OnInstructionFollowed?.Invoke();
        animator.SetTrigger("Followed");
    }

    public void DisableInstruction()
    {
        gameObject.SetActive(false);
    }
}
