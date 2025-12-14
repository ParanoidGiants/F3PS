using System;
using UnityEngine;

public class OnEnemyDied : MonoBehaviour
{
    public event Action<GameObject> OnEnemyDiedEvent;

    public void OnDestroy()
    {
        OnEnemyDiedEvent?.Invoke(gameObject);
    }
}
