using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public static class Helper
{
    public static LayerMask PlayerLayer => LayerMask.GetMask("Character");
    public static LayerMask DefaultLayer => LayerMask.GetMask("Default");
    public static LayerMask ProjectileLayer => LayerMask.GetMask("Projectile");
    public static LayerMask EnemyLayer => LayerMask.GetMask("Enemy");
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
        var vec1XZ = new Vector2(vec1.x, vec1.z);
        var vec2XZ = new Vector2(vec2.x, vec2.z);
        
        return Vector2.Dot(vec1XZ, vec2XZ) > tolerance;
    }

    public static bool IsOnSameY(Vector3 pos1, Vector3 pos2, float tolerance = 0f)
    {
        return Mathf.Abs(pos1.y - pos2.y) < tolerance;
    }
}