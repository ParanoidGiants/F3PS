using System;
using UnityEngine;

public class InstructionElementController : MonoBehaviour
{
    public TMPro.TextMeshProUGUI instructionText;
    public Animator animator;
    public Action OnInstructionFollowed;
    private void OnEnable()
    {
        animator.SetTrigger("Show");
    }

    public void SetupInstructionToFollow(string text)
    {
        instructionText.text = text;
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
