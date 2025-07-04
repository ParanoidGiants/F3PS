using F3PS;
using StarterAssets;
using UnityEngine;

public class UnlockSkill : MonoBehaviour
{
    public Skill skill;

    private void OnTriggerEnter(Collider other)
    {
        if (skill == Skill.None)
        {
            Debug.LogError("Cannot unlock Skill.None.");
            return;
        }

        if (!other.TryGetComponent<ThirdPersonController>(out var player))
        {
            return;
        }

        if (GameManager.Instance.PlayerData.UnlockedSkills.Contains(skill))
        {
            return;
        }

        GameManager.Instance.PlayerEventController.UnlockSkill(skill);
        Destroy(gameObject);
    }
}
