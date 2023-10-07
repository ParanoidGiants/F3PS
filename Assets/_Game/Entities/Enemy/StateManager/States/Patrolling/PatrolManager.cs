using UnityEngine;

public class PatrolManager : MonoBehaviour
{
    private Transform _patrolPointParent;
    
    [Header("Watchers")]
    [SerializeField] private Transform[] _patrolPoints;

    [SerializeField] private int enemyId;
    public int EnemyId => enemyId;
    
    private int _currentPatrolPointIndex;
    public Vector3 CurrentPatrolPoint => _patrolPoints[_currentPatrolPointIndex].position;

    public void Init()
    {
        enemyId = transform.parent.gameObject.GetInstanceID();
        _patrolPointParent = transform;
        
        gameObject.name = transform.parent.name + " " + gameObject.name;
        
        ResetPatrolPoints();
        
        _currentPatrolPointIndex = 0;
    }

    private void ResetPatrolPoints()
    {
        _patrolPoints = new Transform[_patrolPointParent.childCount];
        var patrolPoints = _patrolPointParent.GetComponentInChildren<Transform>();
        for (var i = 0; i < patrolPoints.childCount; i++)
        {
            _patrolPoints[i] = patrolPoints.GetChild(i);
        }
    }

    public void SetNextPatrolPoint()
    {
        _currentPatrolPointIndex++;
        if (_currentPatrolPointIndex >= _patrolPoints.Length)
        {
            _currentPatrolPointIndex = 0;
        }
        ResetPatrolPoints();
    }

    public void SetNextPatrolPointToClosestPoint(Vector3 enemyPosition)
    {
        int closestPatrolPointIndex = 0;
        for (int i = 1; i < _patrolPoints.Length; i++)
        {
            if (Vector3.Distance(enemyPosition, _patrolPoints[i].position) < Vector3.Distance(enemyPosition, _patrolPoints[closestPatrolPointIndex].position))
            {
                closestPatrolPointIndex = i;
            }
        }
        _currentPatrolPointIndex = closestPatrolPointIndex;
        ResetPatrolPoints();
    }
}
