using StarterAssets;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    private ThirdPersonController _playerController;
    private ThirdPersonController PlayerController
    {
        get
        {
            if (_playerController) return _playerController;

            _playerController = FindObjectOfType<ThirdPersonController>();
            return _playerController;
        }
    }

    public GameObject StandardCamera;
    public GameObject AimCamera;
    public GameObject SprintCamera;

    private void Update()
    {
        if (PlayerController.isAiming)
        {
            StandardCamera.SetActive(false);
            SprintCamera.SetActive(false);
            AimCamera.SetActive(true);
        }
        else if (PlayerController.isSprinting)
        {
            StandardCamera.SetActive(false);
            AimCamera.SetActive(false);
            SprintCamera.SetActive(true);
        }
        else
        {
            SprintCamera.SetActive(false);
            AimCamera.SetActive(false);
            StandardCamera.SetActive(true);
        }
    }
}
