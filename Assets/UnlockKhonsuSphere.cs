using F3PS;
using StarterAssets;
using UnityEngine;

public class UnlockKhonsuSphere : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<ThirdPersonController>(out var player))
        {
            return;
        }

        if (GameManager.Instance.PlayerData.UnlockedSkills.Contains(Skill.KhonsuSphere))
        {
            return;
        }

        GameManager.Instance.PlayerEventController.SetKhonsuSphereSkillUnlocked(true);
        Destroy(gameObject);
    }
}
