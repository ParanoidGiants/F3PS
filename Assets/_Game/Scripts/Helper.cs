using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public static class Helper
{
    public static float HALF_PI = Mathf.PI / 2f;
    public static LayerMask PlayerLayer => LayerMask.GetMask("Character");
    public static LayerMask DefaultLayer => LayerMask.GetMask("Default");
    public static LayerMask GroundLayer => LayerMask.GetMask("Ground");
    public static LayerMask ProjectileLayer => LayerMask.GetMask("Projectile");
    public static LayerMask EnemyLayer => LayerMask.GetMask("Enemy");
    public static LayerMask HittableLayer => LayerMask.GetMask("Hittable");

    public static IEnumerator UpdateLayoutGroups(RectTransform rectTransform)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public static bool IsLayerPlayerLayer(int layer)
    {
        var colliderLayer = 1 << layer;
        var result = colliderLayer & PlayerLayer;
        return result != 0;
    }

    public static bool IsLayerDefaultLayer(int layer)
    {
        var colliderLayer = 1 << layer;
        var result = colliderLayer & DefaultLayer;
        return result != 0;
    }

    public static bool IsLayerProjectileLayer(int layer)
    {
        var colliderLayer = 1 << layer;
        var result = colliderLayer & ProjectileLayer;
        return result != 0;
    }
    
    public static bool IsLayerEnemyLayer(int layer)
    {
        var colliderLayer = 1 << layer;
        var result = colliderLayer & EnemyLayer;
        return result != 0;
    }
    public static bool IsInLayerMask(this GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }

    public static bool HasReachedDestination(NavMeshAgent agent, float threshold = 0.1f)
    {
        return !agent.pathPending 
               && agent.remainingDistance <= agent.stoppingDistance + threshold
               && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    public static bool HasReachedStoppingDistance(NavMeshAgent agent, float stoppingDistance, float threshold = 0.1f)
    {
        return !agent.pathPending 
               && agent.remainingDistance <= stoppingDistance + threshold 
               && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    public static bool IsOrientedOnXZ(Vector3 vec1, Vector3 vec2, float tolerance = 0f)
    {
        var vec1XZ = (new Vector2(vec1.x, vec1.z)).normalized;
        var vec2XZ = (new Vector2(vec2.x, vec2.z)).normalized;
        
        var dot = Vector2.Dot(vec1XZ, vec2XZ);
        return (1f-dot) < tolerance;
    }

    public static bool IsOnSameY(Vector3 pos1, Vector3 pos2, float tolerance = 0f)
    {
        return Mathf.Abs(pos1.y - pos2.y) < tolerance;
    }

    public static class Easing
    {
        public static float Linear(float t)
        {
            return t;
        }

        public static float EaseInQuad(float t)
        {
            return t * t;
        }
        public static float EaseOutQuad(float t)
        {
            return t * (2 - t);
        }
    }
    public static float GetPathLengthOnNavMesh(Vector3 originPosition, Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(originPosition, targetPosition, NavMesh.AllAreas, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                float pathLength = 0.0f;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    pathLength += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
                return pathLength;
            }
        }
        return -1f;
    }

    internal static float GetStraightPathLengthOnNavMesh(NavMeshAgent navMeshAgent, Vector3 vector3)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(navMeshAgent.transform.position, vector3, NavMesh.AllAreas, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                float pathLength = 0.0f;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    pathLength += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
                return pathLength;
            }
        }
        return -1f;
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}