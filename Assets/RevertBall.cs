using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class RevertBall : MonoBehaviour
{
    private Transform _targetPlacePoint;
    public Animator _animator;

    public void Init(Transform targetPlacePoint)
    {
        _targetPlacePoint = targetPlacePoint;
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
            gameObject.SetActive(false);
            return;
        }

        if (collision.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {
            player.transform.position = _targetPlacePoint.position;
            gameObject.SetActive(false);
            return;
        }

    }
}