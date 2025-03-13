using StarterAssets;
using UnityEngine;

public class UnlockPistol : MonoBehaviour
{
    public SpawnEnemy spawnEnemy;
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<ThirdPersonController>();
        if (player == null)
        {
            return;
        }

        player.weaponManager.UnlockPistol();
        spawnEnemy.Spawn();
        Destroy(gameObject);
    }
}
