using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStandingGround : MonoBehaviour
{
    // Called when another collider enters this trigger
    void OnTriggerEnter(Collider other)
    {
        var moveWithPlatform = other.GetComponent<ThirdPersonController>();
        if (moveWithPlatform != null)
        {
            moveWithPlatform.SetCurrentPlatform(transform);
        }
    }

    // Called when another collider exits this trigger
    void OnTriggerExit(Collider other)
    {

        var moveWithPlatform = other.GetComponent<ThirdPersonController>();
        if (moveWithPlatform != null)
        {
            moveWithPlatform.RemoveCurrentPlatform(transform);
        }
    }
}
