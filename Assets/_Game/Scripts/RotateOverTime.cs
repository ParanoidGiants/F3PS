using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public float speed = 1.0f;
    void Update()
    {
        Quaternion rotation = Quaternion.AngleAxis(speed * Time.deltaTime, Vector3.up);
        transform.rotation = rotation * transform.rotation;
    }
}
