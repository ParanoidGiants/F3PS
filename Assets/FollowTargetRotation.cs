using UnityEngine;

public class FollowTargetRotation : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        var targetEulerAngles = target.eulerAngles;
        var rotation = Quaternion.Euler(
            targetEulerAngles.x,
            targetEulerAngles.y,
            targetEulerAngles.z
        );

        transform.rotation = rotation;
    }
}
