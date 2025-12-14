using StarterAssets;
using UnityEngine;

public class RevertToPosition : MonoBehaviour
{
    public Transform _revertPlayerToPoint;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {
            player.FreezeAndRevertToPosition(_revertPlayerToPoint.position, _revertPlayerToPoint.rotation);
        }
    }
}
