using UnityEngine;
using StarterAssets;
using F3PS;

[RequireComponent(typeof(InstructionElementController))]
public class InstructionOnJump : MonoBehaviour
{
    private InstructionElementController _instruction;
    private ThirdPersonController _controller;
    private bool _triggered;

    void Awake()
    {
        _controller = FindFirstObjectByType<ThirdPersonController>();
        _instruction = GetComponent<InstructionElementController>();
    }

    void Update()
    {
        if (_triggered) return;

        if (_controller.isAscending)
        {
            _instruction.ProcessFollowedInstruction();
            _triggered = true;
        }
    }
}