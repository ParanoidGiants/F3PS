using UnityEngine;
using StarterAssets;
using F3PS;

[RequireComponent(typeof(InstructionElementController))]
public class InstructionFollowOnMove : MonoBehaviour
{
    [Tooltip("Move vector magnitude required to count as 'moved'")]
    public float threshold = 1f;
    private InstructionElementController _instruction;
    private ThirdPersonController _controller;
    private bool _triggered;
    private Vector3 startPosition;

    void Awake()
    {
        _controller = FindFirstObjectByType<ThirdPersonController>();
        _instruction = GetComponent<InstructionElementController>();
        startPosition = _controller.transform.position;
    }

    void Update()
    {
        if (_triggered) return;

        var horizontalDistance = Vector3.ProjectOnPlane(_controller.transform.position - startPosition, Vector3.up);
        var distanceMagnitude = horizontalDistance.magnitude;


        if (distanceMagnitude > threshold * threshold)
        {
            _instruction.ProcessFollowedInstruction();
            _triggered = true;
        }
    }
}