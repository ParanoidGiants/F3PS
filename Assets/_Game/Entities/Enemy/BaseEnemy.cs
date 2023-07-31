using System;
using StarterAssets;
using UnityEditor.Build;
using UnityEditor.UI;
using UnityEngine;

public enum EnemyState
{
    IDLE,
    SUSPICIOUS,
    DETECTED_TARGET,
    RETURN_TO_IDLE
}

public class BaseEnemy : MonoBehaviour
{
    [Header("References")] 
    public MeshRenderer meshRenderer;
    public Transform detectionSphere;
    public Material idleMaterial;
    public Material suspiciousMaterial;
    public Material detectedTargetMaterial;
    public Material returnToIdleMaterial;

    [Space(10)]
    [Header("Settings")]
    public float moveSpeed;

    [Space(10)]
    [Header("Watchers")]
    public float detectionRadius;
    public EnemyState state;
    public Transform target;

    public bool isPlayerInDetectionRadius;
    
    private Quaternion _originalRotation;
    private Vector3 _originalPosition;
    
    private void Start()
    {
        detectionRadius = detectionSphere.localScale.x;
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (!isPlayerInDetectionRadius) return;

        var targetPosition = target.position + 0.5f * Vector3.up;
        var position = transform.position;
        var direction = targetPosition - position;
        float playerDistance = direction.magnitude;
        direction.Normalize();
        float obstacleDistance = detectionRadius;
        Debug.DrawRay(transform.position, direction * playerDistance, Color.red);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, detectionRadius, Helper.DefaultLayer))
        {
            Debug.Log("Name: " + hit.transform.name);
            Debug.Log("Distance: " + hit.distance);
            obstacleDistance = hit.distance;
        }
        Debug.DrawRay(position, direction * obstacleDistance, Color.green);

        if (playerDistance < obstacleDistance)
        {
            meshRenderer.transform.forward = direction;
            Debug.Log("Player in sight!");
        }
        else
        {
            meshRenderer.transform.forward = _originalRotation * Vector3.forward;
            Debug.Log("Player outta sight!");
        }
    }

    void UpdateState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.IDLE:
                meshRenderer.sharedMaterial = idleMaterial;
                break;
            case EnemyState.SUSPICIOUS:
                meshRenderer.sharedMaterial = suspiciousMaterial;
                break;
            case EnemyState.DETECTED_TARGET:
                meshRenderer.sharedMaterial = detectedTargetMaterial;
                break;
            case EnemyState.RETURN_TO_IDLE:
                meshRenderer.sharedMaterial = returnToIdleMaterial;
                break;
        }
        this.state = state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
        Debug.Log("Player in radius");
        isPlayerInDetectionRadius = true;
        target = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
        Debug.Log("Player out of radius");
        isPlayerInDetectionRadius = false;
        target = null;
        meshRenderer.transform.forward = _originalRotation * Vector3.forward;
    }
}
