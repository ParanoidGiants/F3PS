using UnityEngine;

[RequireComponent(typeof(InstructionElementController))]
public class InstructionOnThrowKhonsuSphere : MonoBehaviour
{
    private InstructionElementController _instruction;
    private KhonsuSphereController _controller;
    private bool _triggered;

    void Awake()
    {
        _controller = FindFirstObjectByType<KhonsuSphereController>();
        _instruction = GetComponent<InstructionElementController>();
    }

    void Update()
    {
        if (_triggered) return;

        if (_controller.isKhonsuSphereActive)
        {
            _instruction.ProcessFollowedInstruction();
            _triggered = true;
        }
    }
}