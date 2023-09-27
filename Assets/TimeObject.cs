using UnityEngine;

public class TimeObject : MonoBehaviour
{
    private const double TOLERANCE = 0.001f;
    private Rigidbody _rb;
    private float _defaultMass;

    public float currentTimeScale;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _defaultMass = _rb.mass;
    }

    void FixedUpdate()
    {
        var scaledGravity = currentTimeScale * Physics.gravity;
        _rb.AddForce(scaledGravity, ForceMode.Acceleration);
    }

    void PitchTimeScale(float newTimeScale)
    {
        var unscaledVelocity = _rb.velocity / currentTimeScale;
        _rb.mass = _defaultMass / (newTimeScale*newTimeScale);
        _rb.velocity = unscaledVelocity * newTimeScale;
        currentTimeScale = newTimeScale;
    }
}
