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
    public float attackStoppingDistance = 1.2f;
    public float defaultStoppingDistance = 0f;
    public float attackDistance = 1f;
    public int maxHealth = 100;
    public int health;

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
        health = maxHealth;
        UpdateState(EnemyState.IDLE);
    }

    private void FixedUpdate()
    {
        bool isTargetInSight = IsTargetInSight();
        if (isTargetInSight && state != EnemyState.DETECTED_TARGET)
        {
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
                break;
            case EnemyState.CHECKING:
                if (navMeshAgent.remainingDistance > 0.1f)
                    break;

                isSuspiciousTime = isSuspiciousTimer;
                _suspiciousRotation = transform.rotation;
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

    public bool IsTargetInSight()
    {
        if (triggerCount == 0) return false;
        
        var position = headMeshRenderer.transform.position;
        
        var targetPosition1 = target.position;
        var direction1 = targetPosition1 - position;
        var playerDistance1 = direction1.magnitude;
        
        direction1.Normalize();
        Debug.DrawRay(position, direction1 * playerDistance1, Color.red);
        RaycastHit hit;
        // check if the player's feet are in sight
        if (Physics.Raycast(position, direction1, out hit, playerDistance1, Helper.DefaultLayer))
        {
            Debug.DrawRay(position, direction1 * playerDistance1, Color.green);
            
            var targetPosition2 = targetPosition1 + Vector3.up;
            var direction2 = targetPosition2 - position;
            var playerDistance2 = direction2.magnitude;
            direction2.Normalize();
            // check if the player's head is in sight
            if (Physics.Raycast(position, direction2, out hit, playerDistance2, Helper.DefaultLayer))
            {
                Debug.DrawRay(position, direction2 * playerDistance2, Color.green);
                return false;
            }
        }
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
                navMeshAgent.stoppingDistance = defaultStoppingDistance;
                headMeshRenderer.sharedMaterial = checkingMaterial;
                break;
            case EnemyState.SUSPICIOUS:
                navMeshAgent.speed = patrolSpeed;
                navMeshAgent.stoppingDistance = defaultStoppingDistance;
                headMeshRenderer.sharedMaterial = suspiciousMaterial;
                break;
            case EnemyState.DETECTED_TARGET:
                navMeshAgent.speed = detectedSpeed;
                navMeshAgent.stoppingDistance = attackStoppingDistance;
                headMeshRenderer.sharedMaterial = detectedTargetMaterial;
                break;
            case EnemyState.RETURN_TO_IDLE:
                navMeshAgent.speed = patrolSpeed;
                navMeshAgent.stoppingDistance = defaultStoppingDistance;
                headMeshRenderer.sharedMaterial = returnToIdleMaterial;
                break;
        }
        this.state = state;
    }

    public int triggerCount = 0;

    private void OnCollisionEnter(Collision other)
    {
        if (!Helper.IsLayerProjectileLayer(other.gameObject.layer)) return;
        

        var projectile = other.gameObject.GetComponent<Projectile>();
        health -= projectile.damage;
        Debug.Log("Hit by projectile");
        if (health > 0)
        {
            FindObjectOfType<EnemyHealthUIPool>().OnHitTarget(this);
        }
        else
        {
            FindObjectOfType<EnemyHealthUIPool>().OnKillTarget(transform);
            Destroy(gameObject);
        }
    }

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
