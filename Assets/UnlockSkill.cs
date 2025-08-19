using F3PS;
using StarterAssets;
using UnityEngine;

public class UnlockSkill : MonoBehaviour
{
    public Skill skill;

    private void Awake()
    {
        if (GameManager.Instance.GameData.PlayerData.UnlockedSkills.Contains(skill))
        {
            Destroy(gameObject);
        }
    }

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

        if (GameManager.Instance.GameData.PlayerData.UnlockedSkills.Contains(skill))
        {
            return;
        }

        GameManager.Instance.GameData.PlayerEventController.UnlockSkill(skill);
        Destroy(gameObject);
    }
}
