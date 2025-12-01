using StarterAssets;
using UnityEngine;

public class RevertToPosition : MonoBehaviour
{
    public Transform _revertPlayerToPoint;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {
            player.FreezeAndRevertToPosition(_revertPlayerToPoint.position, _revertPlayerToPoint.rotation);
            return;
        }
    }
}
