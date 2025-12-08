using UnityEngine;
using StarterAssets;
using F3PS;

[RequireComponent(typeof(InstructionElementController))]
public class InstructionOnSprint : MonoBehaviour
{
    public float sprintingTime = 0f;
    public float sprintingTimer = 3f;
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

        if (!_controller.isSprinting)
        {
            sprintingTime = 0f;
            return;
        }

        sprintingTime += Time.deltaTime;
        if (sprintingTime >= sprintingTimer)
        {
            _instruction.ProcessFollowedInstruction();
            _triggered = true;
        }
    }
}