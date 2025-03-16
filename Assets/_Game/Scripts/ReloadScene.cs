using StarterAssets;
using UnityEngine;

public class ReloadScene : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<ThirdPersonController>();
        if (player != null)
        {
            SceneLoader.Instance.ReloadScene();
        }
    }
}
