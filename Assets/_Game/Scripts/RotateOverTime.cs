using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public Vector3 rotationAxis;
    public float speed = 1.0f;
    void Update()
    {
        Quaternion rotation = Quaternion.AngleAxis(speed * Time.deltaTime, rotationAxis);
        transform.rotation = rotation * transform.rotation;
    }
}
