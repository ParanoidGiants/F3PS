using UnityEngine;
using StarterAssets;
using F3PS;

[RequireComponent(typeof(InstructionElementController))]
public class InstructionOnDodge : MonoBehaviour
{
    private InstructionElementController _instruction;
    private ThirdPersonController _controller;
    private bool _triggered;
    private Vector3 startPosition;

    void Awake()
    {
        _controller = FindFirstObjectByType<ThirdPersonController>();
        _instruction = GetComponent<InstructionElementController>();
    }

    void Update()
    {
        if (_triggered) return;

        if (_controller.isDodging)
        {
            _instruction.ProcessFollowedInstruction();
            _triggered = true;
        }
    }
}