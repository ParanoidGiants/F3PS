using System;
using F3PS;
using TimeBending;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using DarkTonic.MasterAudio;
using Weapon;

namespace Player
{
    public class Extensions : MonoBehaviour
    {
        [Header("Watchers")]
        public bool isAiming;
        public bool isSprinting;
        public bool isShooting;
        public bool isReloading;
        public bool isSwitchingWeapon;
        public bool isDodging;
        public bool isSlowMoToggle;
        public bool isSlowMoStarted;
        public float rotationVelocity;

        public float deltaTime;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;


        [Space(10)]
        [Header("References")]
        public Transform playerSpace;
        public StaminaManager staminaManager;
        public TimeManager _timeManager;
        public WeaponManager weaponManager;
        private Animator _animator;
        public float aimingRotationSpeed;
        public float defaultRotationSpeed;
        private Crosshair _crosshair;

        [Space(10)]
        [Header("Settings")]
        public float health = 100;
        public float maxHealth = 100;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("How fast the character turns to face movement direction when aiming")]
        [Range(0.0f, 1.0f)]
        public float AmingRotationSmoothTime = 0.3f;

        [Tooltip("Move speed of the character in m/s")]
        public float FocusSpeed = 1.0f;

        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        private PlayerHealthUI _playerHealthUI;
        private Camera _mainCamera;
        private readonly int Dodge = Animator.StringToHash("Dodge");
        public float RotationSpeed => isAiming ? aimingRotationSpeed : defaultRotationSpeed;
        
        public void Init(Animator animator)
        {
            _animator = animator;
            
            _playerHealthUI = FindObjectOfType<PlayerHealthUI>();
            _mainCamera = Camera.main;
            _crosshair = FindObjectOfType<Crosshair>();
        }

        private void Start()
        {
            weaponManager.Init(playerSpace);
        }

        // Update is called once per frame
        public void OnUpdate(StarterAssets.StarterAssetsInputs _input)
        {
            if (!weaponManager.ActiveWeapon.isReloadingMagazine)
            {
                weaponManager.HandleSwitchWeapon(_input.switchWeapon, _input.look.x);
            }

            if (GameManager.Instance.timeManager.IsPaused) return;
            
            GroundedCheck();
            isShooting = _input.shoot && !isSprinting;
            isSwitchingWeapon = _input.switchWeapon;
            isReloading = _input.reload;
            Dodging(_input.dodge);
            weaponManager.OnUpdate(isShooting, isReloading, _crosshair.GetTargetPosition());
            UpdateStaminaManager(_input.move.magnitude, _input.aim, _input.sprint);
            UpdateTimeManager(_input.slowmo);
        }

        private void UpdateTimeManager(bool slowMoInput)
        {
            if (!isSlowMoToggle && slowMoInput)
            {
                isSlowMoStarted = !isSlowMoStarted;
                if (isSlowMoStarted)
                {
                    _timeManager.StartSlowMotion();
                }
                else
                {
                    _timeManager.StopSlowMotion();
                }
            }
            isSlowMoToggle = slowMoInput;
        }

        private void UpdateStaminaManager(float moveInput, bool aimInput, bool sprintInput)
        {
            if (staminaManager._isRegenerating)
            {
                isAiming = false;
                isSprinting = false;
            }
            else
            {
                isAiming = aimInput;
                isSprinting = !isAiming && sprintInput;
            }
            staminaManager.UpdateSprinting(isSprinting && moveInput > 0.1f);
            staminaManager.UpdateAiming(isAiming);
        }

        public float GetLookRotation(Transform transform, float movementYaw, float cameraYaw)
        {
            float smoothTime = isAiming ? AmingRotationSmoothTime : RotationSmoothTime;
            float yaw = isSprinting ? movementYaw : cameraYaw;
            return Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                yaw,
                ref rotationVelocity,
                smoothTime * Time.unscaledDeltaTime
            );
        }

        private void Dodging(bool dodgeInput)
        {
            var dodge = Grounded && dodgeInput;
            _animator.SetBool(Dodge, dodge);
        }

        internal float GetTargetSpeed(Vector2 moveVector)
        {
            if (moveVector == Vector2.zero)
            {
                return 0.0f;
            }
            else if (isAiming)
            {
                return FocusSpeed;
            }
            else if (isSprinting)
            {
                return SprintSpeed;
            }
            else
            {
                return MoveSpeed;
            }
        }

        public void Hit(int damage)
        {
            health -= damage; 
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
            _playerHealthUI.UpdateHealth((float)health / maxHealth);
            if (health <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}
