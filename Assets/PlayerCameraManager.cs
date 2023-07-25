using StarterAssets;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private StarterAssetsInputs Input
    {
        get
        {
            if (_input) return _input;

            _input = FindObjectOfType<ThirdPersonController>().Input;
            return _input;
        }
    }

    public GameObject StandardCamera;
    public GameObject AimCamera;
    public GameObject SprintCamera;

    private void Update()
    {
        if (Input.aim)
        {
            StandardCamera.SetActive(false);
            SprintCamera.SetActive(false);
            AimCamera.SetActive(true);
        }
        else if (Input.sprint)
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
