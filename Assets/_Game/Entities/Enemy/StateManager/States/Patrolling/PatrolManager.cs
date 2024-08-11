using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatrolManager : MonoBehaviour
{
    private Transform _patrolPointParent;
    
    [Header("Watchers")]
    [SerializeField] private List<Transform> _patrolPoints;

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
        _patrolPoints = _patrolPointParent.GetComponentsInChildren<Transform>().ToList();
        _patrolPoints.Remove(transform);
        
    }

    public void SetNextPatrolPoint()
    {
        _currentPatrolPointIndex++;
        if (_currentPatrolPointIndex >= _patrolPoints.Count)
        {
            _currentPatrolPointIndex = 0;
        }
    }

    public void SetNextPatrolPointToClosestPoint(Vector3 enemyPosition)
    {
        int closestPatrolPointIndex = 0;
        for (int i = 1; i < _patrolPoints.Count; i++)
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
