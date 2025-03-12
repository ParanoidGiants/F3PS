using StarterAssets;
using UnityEngine;
using Weapon;

public class UnlockPistol : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<ThirdPersonController>();
        if (player == null)
        {
            return;
        }

        player.weaponManager.UnlockPistol();
        Destroy(gameObject);
    }
}
