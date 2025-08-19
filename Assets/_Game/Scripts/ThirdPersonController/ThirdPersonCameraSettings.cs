using F3PS;
using System;
using Unity.Cinemachine;
using UnityEngine;

[Serializable]
public class ThirdPersonCameraSettings
{
    [Header("References")]
    public Transform PlayerCameraTarget;
    public Transform FreeCameraTarget;
    public CinemachineCamera defaultCamera;
    public CinemachineCamera freeCamera;
    public CameraShake cameraShake;

    [Space(10)]
    [Header("Settings")]
    public float CameraTopClamp = 70.0f;
    public float CameraBottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public float freeCameraSpeed = 20f;

    [Space(10)]
    [Header("Watchers")]
    public Transform mainCamera;
    public Transform currentCameraTarget;
    public float cameraTargetYaw;
    public float cameraTargetPitch;
    public bool wasFreeCameraLastFrame = false;
    public bool isFreeCameraActive = false;

    public void Awake()
    {
        mainCamera = Camera.main.transform;
        currentCameraTarget = PlayerCameraTarget;
    }

    public void Spawn(SpawnPointManager spawnPointManager)
    {
        // Get the current spawn point's rotation to ensure camera starts at the correct orientation
        var spawnPoint = spawnPointManager.GetCurrentSpawnPosition();
        cameraTargetYaw = spawnPoint.rotation.eulerAngles.y;
        
        if (defaultCamera != null)
        {
            // Set the camera to invalidate its previous state
            defaultCamera.PreviousStateIsValid = false;

            // Force the camera to immediately snap to its target
            // This makes it appear as if the camera was always at the correct position
            defaultCamera.InternalUpdateCameraState(Vector3.up, -1);
        }

        if (freeCamera != null)
        {
            // Also handle free camera if it exists
            freeCamera.PreviousStateIsValid = false;
            freeCamera.InternalUpdateCameraState(Vector3.up, -1);
        }
    }


    public void HandleFreeCamera()
    {
        bool isFreeCameraThisFrame = GameManager.Instance.inputs.freeCamera;
        bool isKeyDown = !wasFreeCameraLastFrame && isFreeCameraThisFrame;
        wasFreeCameraLastFrame = isFreeCameraThisFrame;
        if (isKeyDown && !isFreeCameraActive)
        {
            isFreeCameraActive = true;
            freeCamera.gameObject.SetActive(true);
            defaultCamera.gameObject.SetActive(false);
            currentCameraTarget = FreeCameraTarget;
            FreeCameraTarget.position = defaultCamera.transform.position;

            GameManager.Instance.ActivateFreeCamera();
        }
        else if (isKeyDown && isFreeCameraActive)
        {
            isFreeCameraActive = false;
            freeCamera.gameObject.SetActive(false);
            defaultCamera.gameObject.SetActive(true);
            currentCameraTarget = PlayerCameraTarget;

            GameManager.Instance.DeactivateFreeCamera();
        }
        else if (isFreeCameraActive)
        {
            var move = GameManager.Instance.inputs.move;
            var shoot = GameManager.Instance.inputs.shoot;
            var speed = (shoot ? 2f : 1f) * freeCameraSpeed;
            var moveDirection = (move.x * FreeCameraTarget.right + move.y * FreeCameraTarget.forward).normalized;
            FreeCameraTarget.position += speed * Time.unscaledDeltaTime * moveDirection;
        }
    }

    private const float _threshold = 0.01f;
    public void CameraTargetRotation()
    {
        var look = GameManager.Instance.inputs.look;
        if (look.sqrMagnitude >= _threshold)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplierPitch = GameManager.Instance.IsCurrentDeviceMouse
                ? 1.0f
                : Time.unscaledDeltaTime * GameManager.Instance.GameData.PlayerData.RotationSpeedPitch;
            float deltaTimeMultiplierYaw = GameManager.Instance. IsCurrentDeviceMouse
                ? 1.0f
                : Time.unscaledDeltaTime * GameManager.Instance.GameData.PlayerData.RotationSpeedYaw;

            cameraTargetYaw += look.x * deltaTimeMultiplierYaw;
            cameraTargetPitch += look.y * deltaTimeMultiplierPitch;
        }

        // clamp our rotations so our values are limited 360 degrees
        cameraTargetYaw = ClampAngle(cameraTargetYaw, float.MinValue, float.MaxValue);
        cameraTargetPitch = ClampAngle(cameraTargetPitch, CameraBottomClamp, CameraTopClamp);

        // Cinemachine will follow this target
        currentCameraTarget.rotation = Quaternion.Euler(cameraTargetPitch, cameraTargetYaw, 0.0f);
    }

    private float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public float GetTargetYawFromInputDirection(Vector3 lastInputDirection)
    {
        return Mathf.Atan2(lastInputDirection.x, lastInputDirection.z) * Mathf.Rad2Deg
                 + mainCamera.transform.eulerAngles.y;
    }
}
