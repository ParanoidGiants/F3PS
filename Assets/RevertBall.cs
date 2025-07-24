using System;
using StarterAssets;
using UnityEngine;

public class RevertBall : MonoBehaviour
{
    private Rigidbody _rb;
    private Transform _targetPlacePoint;
    public Transform _spawnPoint;
    public float _lifeTime;
    public float _lifeDuration;

    public bool isInitialized = false;

    public void Init(Transform targetPlacePoint, Transform spawnPoint, float lifeTime)
    {
        _targetPlacePoint = targetPlacePoint;
        _spawnPoint = spawnPoint;
        _lifeDuration = lifeTime;
        isInitialized = true;
    }

    private void Update()
    {
        _lifeTime += Time.deltaTime;
        if (_lifeTime >= _lifeDuration)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {

            return;
        }
        player.transform.position = _targetPlacePoint.position;
        gameObject.SetActive(false);

    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            return;
        }
        _lifeTime = 0f;
        transform.position = _spawnPoint.position;
        transform.rotation = Quaternion.identity;
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = Vector3.down;
        _rb.angularVelocity = Vector3.zero;
    }
}