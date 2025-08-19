using F3PS;
using StarterAssets;
using UnityEngine;

public class UnlockAttack : MonoBehaviour
{
    public Attack attack;

    private void Awake()
    {
        if (GameManager.Instance.GameData.PlayerData.UnlockedAttacks.Contains(attack))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attack == Attack.None)
        {
            Debug.LogError("Cannot unlock Attack.None.");
            return;
        }

        if (!other.TryGetComponent<ThirdPersonController>(out var player))
        {
            return;
        }

        if (GameManager.Instance.GameData.PlayerData.UnlockedAttacks.Contains(attack))
        {
            return;
        }

        GameManager.Instance.GameData.PlayerEventController.UnlockAttack(attack);
        Destroy(gameObject);
    }
}
