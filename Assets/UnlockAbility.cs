using F3PS;
using StarterAssets;
using UnityEngine;

public class UnlockAbility : MonoBehaviour
{
    public Ability ability;

    private void OnTriggerEnter(Collider other)
    {
        if (ability == Ability.None)
        {
            Debug.LogError("Cannot unlock Ability.None.");
            return;
        }

        if (!other.TryGetComponent<ThirdPersonController>(out var player))
        {
            return;
        }

        if (GameManager.Instance.GameData.PlayerData.UnlockedAbilities.Contains(ability))
        {
            return;
        }

        GameManager.Instance.GameData.PlayerEventController.UnlockAbility(ability);
        Destroy(gameObject);
    }
}
