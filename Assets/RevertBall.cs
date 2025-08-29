using StarterAssets;
using UnityEngine;

public class RevertBall : MonoBehaviour
{
    private Transform _revertPlayerToPoint;
    public Animator _animator;

    public void Init(Transform revertPlayerToPoint)
    {
        _revertPlayerToPoint = revertPlayerToPoint;
    }

    public void StartRun(float speed)
    {
        _animator.speed = speed;
        _animator.SetTrigger("Start");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<HorusPalmProjectile>(out var _) || collision.gameObject.TryGetComponent<OsirisKickProjectile>(out var _))
        {
            Deactivate();
            return;
        }

        if (collision.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {
            player.FreezeAndRevertToPosition(_revertPlayerToPoint.position, _revertPlayerToPoint.rotation);
            Deactivate();
            return;
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}