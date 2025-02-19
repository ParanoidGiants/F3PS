using F3PS;
using UnityEngine;
using TimeBending;

using Weapon;


#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using DarkTonic.MasterAudio;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    // [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("References")]
        public Transform playerSpace;
        public StaminaManager staminaManager;
        public TimeManager timeManager;
        public WeaponManager weaponManager;
        public CameraShake cameraShake;
        public AnimateMesh animateMesh;
        private Crosshair _crosshair;
        private PlayerHealthUI _playerHealthUI;

        [Space(20)]
        [Header("Settings")]
        [Header("Grounded")]

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;


        [Space(10)]
        [Header("Health")]
        [Tooltip("Maximum Health")]
        [Range(0f, 100f)]
        public float maxHealth = 100;


        [Space(10)]
        [Header("Move")]
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("How fast the character turns to face movement direction when aiming")]
        [Range(0.0f, 1.0f)]
        public float AimingRotationSmoothTime = 0.3f;

        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("How fast the camera can revolve around the player")]
        public float RotationSpeedY = 0.2f;
        public float RotationSpeedX = 0.2f;


        [Space(10)]
        [Header("Gravity, Jump & Dodge")]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpCoolDownTimer = 0.50f;
        private float _jumpCoolDownTime;
        
        [Tooltip("The jump height of the player while dodging")]
        public float DodgeHeight = 1.2f;
        
        [Tooltip("The jump length of the player while dodging")]
        public float DodgeSpeed = 60f; 
        
        [Tooltip("The time it takes for the dodge speed to cool off")]
        public float DodgeAscendTimer = 0.5f;
        [SerializeField]
        private float _dodgeAscendTime;
        [Tooltip("The time it takes for the dodge roll landing animation speed to cool off")]
        public float DodgeLandTimer = 0.5f;
        [SerializeField]
        private float _dodgeLandTime;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Tooltip("The time it takes to dodge again after landing from a dodge")]
        public float DodgeCoolDownTimer = 0.25f;
        private float _dodgeCoolDownTime;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        private float _fallTimeoutDelta;
        
        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float CameraTopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float CameraBottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;


        [Space(10)]
        
        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        [Header("Watchers")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] private bool _isGrounded = true;
        [SerializeField] private bool _isSprinting;
        [SerializeField] private bool _isShooting;
        [SerializeField] private bool _isReloading;
        [SerializeField] private bool _isDodging;
        [SerializeField] private bool _isSlowMoToggle;
        [SerializeField] private bool _isSlowMoStarted;
        [SerializeField] private bool _isAimingGrenade;
        [SerializeField] private bool _isRestartingGame;
        [SerializeField] private bool _isDying;
        [SerializeField] private float _rotationVelocity;
        [SerializeField] private float _health;
        [SerializeField] private float _speed;
        [SerializeField] private float _animationBlend;
        [SerializeField] private float _targetYaw;
        [SerializeField] private float _lookYaw;
        [SerializeField] private float _verticalVelocity;
        [SerializeField] private Vector3 _lastInputDirection;

        private const float _threshold = 0.01f;
        private const float _terminalVelocity = 53.0f;
        private bool _hasAnimator;

        [Header("Audio")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        // animation IDs
        private readonly int _animIDSpeed = Animator.StringToHash("Speed");
        private readonly int _animIDGrounded = Animator.StringToHash("Grounded");
        private readonly int _animIDJump = Animator.StringToHash("Jump");
        private readonly int _animIDFreeFall = Animator.StringToHash("FreeFall");
        private readonly int _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        private readonly int _animIDDodge = Animator.StringToHash("Dodge");
        private readonly int _animIDHit = Animator.StringToHash("Hit");


#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        [SerializeField] private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        [SerializeField] private StarterAssetsInputs _input;
        public StarterAssetsInputs Input => _input;
        private GameObject _mainCamera;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            // _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            // _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
            _playerHealthUI = FindObjectOfType<PlayerHealthUI>();
            _crosshair = FindObjectOfType<Crosshair>();

            _health = maxHealth;
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            // reset our timeouts on start
            _jumpCoolDownTime = JumpCoolDownTimer;
            _fallTimeoutDelta = FallTimeout;
            _dodgeCoolDownTime = DodgeCoolDownTimer;

            weaponManager.Init(playerSpace);
        }

        private void Update()
        {
            if (GameManager.Instance.IsGamePaused) return;

            if (_input.restart && !_isRestartingGame && !_isDying)
            {
                SceneLoader.Instance.ReloadScene();
                _isRestartingGame = true;
            }

            if (!weaponManager.ActiveWeapon.isReloadingMagazine)
            {
                weaponManager.HandleSwitchWeapon(_input.switchWeapon, _input.look.x);
            }

            if (GameManager.Instance.timeManager.IsPaused) return;

            GroundedCheck();
            _isShooting = _input.shoot;
            _isReloading = _input.reload;
            _isAimingGrenade = weaponManager.grenade.gameObject.activeSelf && _input.aimGrenade;
            weaponManager.OnUpdate(
                _isAimingGrenade,
                _isShooting,
                _isReloading,
                _crosshair.GetTargetPosition()
            );
            UpdateStaminaManager(_input.move.magnitude, _isAimingGrenade, _input.sprint);
            UpdateTimeManager(_input.slowmo);

            _hasAnimator = TryGetComponent(out _animator);

            _animator.SetBool(_animIDGrounded, _isGrounded);
            JumpAndGravity();

            if (_isDodging)
            {
                Dodge();
            }
            else
            {
                Move();
            }
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.IsGamePaused) return;
            if (GameManager.Instance.timeManager.IsPaused) return;

            weaponManager.OnFixedUpdate(_crosshair.GetTargetPosition());
        }

        private void LateUpdate()
        {
            if (!GameManager.Instance.IsGamePaused && GameManager.Instance.timeManager.IsPaused) return;

            CameraRotation();
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplierY = IsCurrentDeviceMouse ? 1.0f : Time.unscaledDeltaTime * RotationSpeedY;
                float deltaTimeMultiplierX = IsCurrentDeviceMouse ? 1.0f : Time.unscaledDeltaTime * RotationSpeedX;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplierX;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplierY;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, CameraBottomClamp, CameraTopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
            float targetSpeed = GetTargetSpeed(_input.move);

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset
                || currentHorizontalSpeed > targetSpeed + speedOffset
            ) {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            if (_input.move.sqrMagnitude > 0f)
            {
                _lastInputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            }
            _targetYaw = _mainCamera.transform.eulerAngles.y
                         + Mathf.Rad2Deg * Mathf.Atan2(_lastInputDirection.x, _lastInputDirection.z);
            _lookYaw = GetLookYaw(transform, _targetYaw, _cinemachineTargetYaw);
            
            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
            Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(
                lookDirection.normalized * (_speed * Time.deltaTime)
                + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void Dodge()
        {
            if (_dodgeAscendTime <= 0f && _dodgeLandTime <= 0f)
            {
                _isDodging = false;
                return;
            }
            else if (_isGrounded)
            {
                _dodgeAscendTime = 0;
                _dodgeLandTime -= Time.deltaTime;
            }
            else if (_dodgeAscendTime > 0f)
            {
                _dodgeAscendTime -= Time.deltaTime;
            }
            else
            {
                _dodgeAscendTime = 0f;
            }

            var speedFactor = (_dodgeAscendTime + _dodgeLandTime) / (DodgeAscendTimer + DodgeLandTimer);
            speedFactor = Mathf.Max(speedFactor, 0f);
            _speed = Mathf.Lerp(DodgeSpeed/2f, DodgeSpeed, Mathf.Pow(speedFactor,4f));
            _targetYaw = Mathf.Atan2(_lastInputDirection.x, _lastInputDirection.z) * Mathf.Rad2Deg
                         + _mainCamera.transform.eulerAngles.y;
            _lookYaw = GetLookYaw(transform, _targetYaw, _cinemachineTargetYaw);
            
            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
            Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(
                lookDirection.normalized * (_speed * Time.deltaTime)
                + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );
        }

        private void JumpAndGravity()
        {
            if (_isGrounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                    _animator.SetBool(_animIDDodge, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpCoolDownTime <= 0.0f && _dodgeCoolDownTime <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                        MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
                    }
                }
                
                // Dodge
                else if (!_isDodging && _input.dodge && _jumpCoolDownTime <= 0.0f && _dodgeCoolDownTime <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(DodgeHeight * -2f * Gravity);
                    _isDodging = true;
                    _dodgeAscendTime = DodgeAscendTimer;
                    _dodgeLandTime = DodgeLandTimer;
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDDodge, true);
                        MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
                    }
                }

                // jump timeout
                if (_jumpCoolDownTime >= 0.0f)
                {
                    _jumpCoolDownTime -= Time.deltaTime;
                }
                if (_dodgeCoolDownTime >= 0.0f)
                {
                    _verticalVelocity = Mathf.Max(_verticalVelocity, DodgeHeight);
                    _dodgeCoolDownTime -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                if (_isDodging)
                {
                    _dodgeCoolDownTime = DodgeCoolDownTimer;
                }
                else
                {
                    _jumpCoolDownTime = JumpCoolDownTimer;
                }
                

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }

                // if we are not grounded, do not jump
                _input.jump = false;
                // if we are not grounded, do not jump
                _input.dodge = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity && _dodgeAscendTime <= 0f)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    MasterAudio.PlaySound3DAtTransformAndForget("Player_movement", transform);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void UpdateTimeManager(bool slowMoInput)
        {
            if (!_isSlowMoToggle && slowMoInput)
            {
                _isSlowMoStarted = !_isSlowMoStarted;
                if (_isSlowMoStarted)
                {
                    timeManager.StartSlowMotion();
                }
                else
                {
                    timeManager.StopSlowMotion();
                }
            }
            _isSlowMoToggle = slowMoInput;
        }

        private void UpdateStaminaManager(float moveInput, bool aimInput, bool sprintInput)
        {
            if (staminaManager._isRegenerating)
            {
                _isAimingGrenade = false;
                _isSprinting = false;
            }
            else
            {
                _isAimingGrenade = aimInput;
                _isSprinting = !_isAimingGrenade && sprintInput;
            }
            staminaManager.UpdateSprinting(_isSprinting && moveInput > 0.1f);
            staminaManager.UpdateAiming(_isAimingGrenade);
        }

        public float GetLookYaw(Transform transform, float movementYaw, float cameraYaw)
        {
            float smoothTime = _isAimingGrenade ? AimingRotationSmoothTime : RotationSmoothTime;
            float yaw = _isDodging || _isSprinting ? movementYaw : cameraYaw;
            return Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                yaw,
                ref _rotationVelocity,
                smoothTime * Time.unscaledDeltaTime
            );
        }

        internal float GetTargetSpeed(Vector2 moveVector)
        {
            if (moveVector == Vector2.zero)
            {
                return 0.0f;
            }
            if (_isSprinting)
            {
                return SprintSpeed;
            }
            return MoveSpeed;
        }

        public void Hit(int damage)
        {
            _health -= damage;
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
            _playerHealthUI.UpdateHealth((float)_health / maxHealth);
            if (_health <= 0 && !_isDying)
            {
                _isDying = true;
                Die();
            }
            else
            {
                _animator.SetTrigger(_animIDHit);
            }
            cameraShake.Shake(damage);
            animateMesh.HitFlash();
        }

        private void Die()
        {
            _animator.SetTrigger("Die");
            if (!_isRestartingGame)
            {
                SceneLoader.Instance.ReloadScene();
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            _isGrounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (_isGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}
