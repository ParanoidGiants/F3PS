using StarterAssets;
using UnityEngine;

public class RevertToPosition : MonoBehaviour
{
    public Transform _revertPlayerToPoint;
    private FlashScreenController _flashScreenController;

    private void Awake() { _flashScreenController = FindFirstObjectByType<FlashScreenController>(); }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {
            player.SpawnAt(_revertPlayerToPoint.position, _revertPlayerToPoint.rotation);
            _flashScreenController.CoverScreen();
            _flashScreenController.UncoverScreen();
            return;
        }
    }
}
