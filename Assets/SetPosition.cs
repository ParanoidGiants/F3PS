using UnityEngine;

public class SetPosition : MonoBehaviour
{
    public Vector3 offset = Vector3.up;

    public void SetPositionToSameAsTarget(Transform target)
    {
        transform.position = target.position + offset;
    }
}
