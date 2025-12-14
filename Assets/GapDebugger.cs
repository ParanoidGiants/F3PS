using UnityEngine;

public class GapDebugger : MonoBehaviour
{
    public Transform playerTransform;
    public Transform platformTransform;

    private Collider _playerCollider;
    private Collider _platformCollider;

    void Start()
    {
        _playerCollider = playerTransform.GetComponent<Collider>();
        _platformCollider = platformTransform.GetComponent<Collider>();
    }

    // LateUpdate runs after all physics calculations (FixedUpdate) for the frame are complete.
    void LateUpdate()
    {
        if (playerTransform == null || platformTransform == null) return;

        // Calculate the position of the player's bottom
        float playerBottom = playerTransform.position.y - _playerCollider.bounds.extents.y;

        // Calculate the position of the platform's top
        float platformTop = platformTransform.position.y + _platformCollider.bounds.extents.y;

        // Calculate the gap
        float verticalGap = playerBottom - platformTop;

        // If the gap is positive, it means the player is floating above the platform.
        // We check against a small epsilon to ignore tiny, irrelevant floating-point noise.
        if (verticalGap > 0.001f)
        {
            Debug.LogError($"A gap of {verticalGap} units was created by the physics engine.");
        }
    }
}