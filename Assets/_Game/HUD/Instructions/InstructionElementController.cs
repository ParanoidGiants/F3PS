using System;
using UnityEngine;

public class InstructionElementController : MonoBehaviour
{
    public TMPro.TextMeshProUGUI instructionText;
    public Action OnInstructionFollowed;

    public void SetupInstructionToFollow(string text)
    {
        instructionText.text = text;
    }

    public void ProcessFollowedInstruction()
    {
        OnInstructionFollowed?.Invoke();
        gameObject.SetActive(false);
    }
}
