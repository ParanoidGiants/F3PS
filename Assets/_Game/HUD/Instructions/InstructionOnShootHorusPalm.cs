using UnityEngine;
using StarterAssets;
using F3PS;

[RequireComponent(typeof(InstructionElementController))]
public class InstructionOnShootHorusPalm : MonoBehaviour
{
    private InstructionElementController _instruction;
    private HorusPalmController _controller;
    private bool _triggered;

    void Awake()
    {
        _controller = FindFirstObjectByType<HorusPalmController>();
        _instruction = GetComponent<InstructionElementController>();
    }

    void Update()
    {
        if (_triggered) return;

        if (_controller.isAttacking)
        {
            _instruction.ProcessFollowedInstruction();
            _triggered = true;
        }
    }
}