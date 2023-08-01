using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    IDLE,
    CHECKING,
    SUSPICIOUS,
    DETECTED_TARGET,
    RETURN_TO_IDLE
}

public class BaseEnemy : MonoBehaviour
{
    [Header("References")] 
    public MeshRenderer headMeshRenderer;
    public NavMeshAgent navMeshAgent;

    public Material idleMaterial;
    public Material checkingMaterial;
    public Material suspiciousMaterial;
    public Material detectedTargetMaterial;
    public Material returnToIdleMaterial;
    
    [Space(10)]
    [Header("Settings")]
    public float maxRaycastDistance;
    public float isSuspiciousTimer;
    public float isSuspiciousTime;
    public float detectedSpeed = 3f;
    public float patrolSpeed = 1f;
    public float attackDistance = 1f;


    [Space(10)]
    [Header("Watchers")]
    public EnemyState state;
    public Transform target;

    private Quaternion _originalRotation;
    private Vector3 _originalPosition;
    private Quaternion _suspiciousRotation;
    
    private void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        UpdateState(EnemyState.IDLE);
    }

    private void FixedUpdate()
    {
        bool isTargetInSight = IsTargetInSight(out Vector3 targetDirection);
        if (isTargetInSight && state != EnemyState.DETECTED_TARGET)
        {
            navMeshAgent.speed = detectedSpeed;
            UpdateState(EnemyState.DETECTED_TARGET);
        }
        

        switch (state)
        {
            case EnemyState.IDLE:
                transform.rotation = Quaternion.Slerp(transform.rotation, _originalRotation, Time.deltaTime * 10f);
                transform.position = _originalPosition;
                break;
            case EnemyState.DETECTED_TARGET:
                if (!isTargetInSight)
                {
                    UpdateState(EnemyState.CHECKING);
                    break;
                }
                
                navMeshAgent.destination = target.position;
                if (navMeshAgent.remainingDistance <= attackDistance)
                {
                    navMeshAgent.speed = 0f;
                }
                break;
            case EnemyState.CHECKING:
                if (navMeshAgent.remainingDistance > 0.1f)
                    break;

                isSuspiciousTime = isSuspiciousTimer;
                _suspiciousRotation = transform.rotation;
                navMeshAgent.speed = patrolSpeed;
                UpdateState(EnemyState.SUSPICIOUS);
                break;
            case EnemyState.SUSPICIOUS:
                isSuspiciousTime -= Time.deltaTime;
                
                float isSuspiciousPercenatge = isSuspiciousTime / isSuspiciousTimer;
                float isSuspiciousAnimateTime = Mathf.Sin(isSuspiciousPercenatge * (2f * Mathf.PI));
                transform.rotation = _suspiciousRotation * Quaternion.Euler(0, 30 * isSuspiciousAnimateTime, 0f);

                if (isSuspiciousTime > 0f) break; 
                
                isSuspiciousTime = 0;
                navMeshAgent.destination = _originalPosition;
                UpdateState(EnemyState.RETURN_TO_IDLE);
                break;
            case EnemyState.RETURN_TO_IDLE:
                if (navMeshAgent.remainingDistance > 0.1f)
                    break;
                UpdateState(EnemyState.IDLE);
                break;
        }
    }

    public bool IsTargetInSight(out Vector3 targetDirection)
    {
        targetDirection = Vector3.zero;
        if (triggerCount == 0) return false;
        
        var targetPosition = target.position + 0.5f * Vector3.up;
        var position = headMeshRenderer.transform.position;
        var direction = targetPosition - position;
        float playerDistance = direction.magnitude;
        
        direction.Normalize();
        Debug.DrawRay(position, direction * playerDistance, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(position, direction, out hit, playerDistance, Helper.DefaultLayer))
        {
            Debug.Log(hit.collider.name);
            Debug.DrawRay(position, direction * playerDistance, Color.green);
            return false;
        }

        targetDirection = direction;
        return true;
    }

    void UpdateState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.IDLE:
                headMeshRenderer.sharedMaterial = idleMaterial;
                break;
            case EnemyState.CHECKING:
                headMeshRenderer.sharedMaterial = checkingMaterial;
                break;
            case EnemyState.SUSPICIOUS:
                headMeshRenderer.sharedMaterial = suspiciousMaterial;
                break;
            case EnemyState.DETECTED_TARGET:
                headMeshRenderer.sharedMaterial = detectedTargetMaterial;
                break;
            case EnemyState.RETURN_TO_IDLE:
                headMeshRenderer.sharedMaterial = returnToIdleMaterial;
                break;
        }
        this.state = state;
    }

    public int triggerCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
        triggerCount++;
        target = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
        triggerCount--;
        
        if (triggerCount > 0) return;
        target = null;
    }
}
