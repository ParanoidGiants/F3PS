using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<ThirdPersonController>();
        if (player == null)
        {
            return;
        }

        // player.ResetToLastGroundPosition();
    }
}
