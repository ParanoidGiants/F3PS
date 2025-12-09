using UnityEngine;

[RequireComponent(typeof(InstructionElementController))]
public class InstructionOnThrowKhonsuSphere : MonoBehaviour
{
    private InstructionElementController _instruction;
    private KhonsuSphereProjectile _controller;
    private bool _triggered;

    void Awake()
    {
        _controller = FindFirstObjectByType<KhonsuSphereProjectile>(FindObjectsInactive.Include);
        _instruction = GetComponent<InstructionElementController>();
    }

    void Update()
    {
        if (_triggered) return;

        if (_controller.isUpAndRunning)
        {
            _instruction.ProcessFollowedInstruction();
            _triggered = true;
        }
    }
}