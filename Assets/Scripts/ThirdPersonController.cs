using F3PS;
using UnityEngine;
using Cinemachine;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using DarkTonic.MasterAudio;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(Rigidbody))]
    public class ThirdPersonController : MonoBehaviour
    {
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        [Header("Data")]
        public PlayerData playerModel;
        public PlayerEventController playerEventController;

        #region DEBUG_TOOLS
        [Space(20)]
        [Header("Debug Pause and Camera")]
        public GameObject freeCameraText;
        public GameObject pausedText;
        public GameObject slowMoText;

        public CanvasGroup uiCanvasGroup;

        public Transform _currentCameraTarget;
        public CinemachineVirtualCamera freeCamera;
        public Transform freeCameraTarget;
        public float freeCameraSpeed = 20f;
        public bool canControlPlayer = true;

        #endregion DEBUG_TOOLS

        [Space(20)]
        [Header("References")]
        private bool _hasAnimator;
        public Animator animator;
        public CinemachineVirtualCamera defaultCamera;
        public StaminaManager staminaManager;
        public SkillManager skillManager;
        public CameraShake cameraShake;
        public AnimateMesh animateMesh;
        public HittableManager hittableManager;
        public Transform armature;


        [Space(20)]
        [Header("Settings")]
        public float jumpCoolDownTimer = 0.25f;
        public float jumpCoolDownTime;
        public float dodgeCoolDownTimer = 0.25f;
        public float dodgeCoolDownTime;
        public float fallTimer = 0.15f;
        public float fallTime;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;


        [Tooltip("The time it takes for the dodge speed to cool off")]
        public float DodgeAscendTimer = 0.5f;

        [Header("Gravity, Jump & Dodge")]
        public float groundedCoyoteDuration = 0.3f;
        public float _groundedCoyoteTime = 0f;
        public float _dodgeAscendTime;
        [Tooltip("The time it takes for the dodge roll landing animation speed to cool off")]
        public float DodgeLandTimer = 0.5f;
        public float _dodgeLandTime;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;
        
        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public Transform PlayerCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float CameraTopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float CameraBottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Space(10)]
        [Header("Platform")]
        public Transform currentGround;
        public Vector3 groundNormal;
        public Vector3 lastGroundPosition;

        [Header("Watchers")]
        [SerializeField] private bool _isGrounded;
        [SerializeField] private bool _isSprinting;
        [SerializeField] private bool _isShooting;
        [SerializeField] private bool _isReloading;
        [SerializeField] private bool _isDodging;
        [SerializeField] private bool _isSlowMoToggle;
        [SerializeField] private bool _isSlowMoStarted;
        [SerializeField] private bool _isAimingGrenade;
        [SerializeField] private bool _isDying;
        [SerializeField] private bool _wasTimeStoppedLastFrame = false;
        [SerializeField] private bool _isTimeStopped = false;
        [SerializeField] private float _rotationVelocity;
        [SerializeField] private float _speed;
        [SerializeField] private float _animationBlend;
        [SerializeField] private float _targetYaw;
        [SerializeField] private float _lookYaw;
        [SerializeField] private float _verticalVelocity;
        [SerializeField] private Vector3 _lastInputDirection;

        private const float _threshold = 0.01f;
        private const float _terminalVelocity = 53.0f;

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

        private GameObject _mainCamera;
        private Rigidbody _rigidbody;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private StarterAssetsInputs _input;
        public StarterAssetsInputs Input => _input;

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

        public bool IsGrounded => _isGrounded;

        private void Awake()
        {
            _mainCamera = FindObjectOfType<Camera>().gameObject;
            _hasAnimator = animator != null;
            _rigidbody = GetComponent<Rigidbody>();
            _input = GameManager.Instance.inputs;
            _playerInput = _input.GetComponent<PlayerInput>();
#if !ENABLE_INPUT_SYSTEM || !STARTER_ASSETS_PACKAGES_CHECKED
            LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
            playerModel = GameManager.Instance.PlayerData;
        }

        private void Start()
        {
            _currentCameraTarget = PlayerCameraTarget;
            _cinemachineTargetYaw = PlayerCameraTarget.rotation.eulerAngles.y;
            // reset our timeouts on start
            jumpCoolDownTime = jumpCoolDownTimer;
            fallTime = fallTimer;
            dodgeCoolDownTime = dodgeCoolDownTimer;
            skillManager.Init();
        }
        private void Update()
        {
            HandleMenu();
            HandleFreeCamera();
            HandleStopTime();
            if (!canControlPlayer) return;
            if (_isDying) return;
            if (GameManager.Instance.timeManager.Stopped) return;

            animator.SetBool(_animIDGrounded, _isGrounded);

            skillManager.OnUpdate();
            
            // Update Stamina Manager
            if (staminaManager._isRegenerating)
            {
                _isSprinting = false;
            }
            else
            {
                _isSprinting = !_isAimingGrenade && _input.sprint;
            }
            staminaManager.UpdateSprinting(_isSprinting && _input.move.magnitude > 0.1f);


            UpdateTimeManager(_input.slowmo);
            HandlePlatformTransform();
        }

        private void FixedUpdate()
        {
            if (!canControlPlayer) return;
            if (_isDying) return;
            if (GameManager.Instance.IsGamePaused) return;
            if (GameManager.Instance.timeManager.Stopped) return;


            GroundedCheck();
            HandleFallAndGravity();

            skillManager.OnFixedUpdate();
            if (skillManager.IsAiming())
            {
                MoveWhileAiming();
                return;
            }

            JumpAndDodge();

            if (_isDodging)
            {
                HandleDodgeRoll();
            }
            else
            {
                Move();
            }
        }

        private void LateUpdate()
        {
            if (_isMenuOpen)
                return;

            if (skillManager.telekinesisController.isRotatingObjectThisFrame)
                return;

            CameraTargetRotation();
        }


        private void HandlePlatformTransform()
        {
            if (!_isGrounded)
            {
                return;
            }

            var platformDirection = currentGround.position - lastGroundPosition;
            lastGroundPosition = currentGround.position;
            transform.position += platformDirection;
        }

        public void StopControlPlayer()
        {
            canControlPlayer = false;
            GameManager.Instance.timeManager.StopSlowMotion();
        }
        public void ResumeControlPlayer()
        {
            canControlPlayer = true;
        }


        private bool _wasMenuPressedLastFrame = false;
        private bool _isMenuOpen = false;
        private void HandleMenu()
        {
            if (SceneLoader.Instance.isLoading) return;

            var isMenuPressedThisFrame = _input.menu;
            var isKeyDown = !_wasMenuPressedLastFrame && isMenuPressedThisFrame;
            _wasMenuPressedLastFrame = isMenuPressedThisFrame;
            if (isKeyDown && !_isMenuOpen)
            {
                OpenMenu();
            }
            else if (isKeyDown && _isMenuOpen)
            {
                ResumeGame();
            }
        }

        public void OpenMenu()
        {
            GameManager.Instance.PauseGame();
            GameManager.Instance.OpenMenu();
            canControlPlayer = false;
            _isMenuOpen = true;
        }

        public void ResumeGame()
        {
            if (!_isTimeStopped)
            {
                GameManager.Instance.ResumeGame();
            }
            GameManager.Instance.CloseMenu();
            canControlPlayer = true;
            _isMenuOpen = false;
        }

        private void HandleStopTime()
        {
            if (_isMenuOpen) return;

            bool isTimeStoppedThisFrame = _input.pause;
            bool isKeyDown = !_wasTimeStoppedLastFrame && isTimeStoppedThisFrame;
            _wasTimeStoppedLastFrame = isTimeStoppedThisFrame;
            if (isKeyDown && !_isTimeStopped)
            {
                pausedText.SetActive(true);
                GameManager.Instance.PauseGame();
                _isTimeStopped = true;
            }
            else if (isKeyDown && _isTimeStopped)
            {
                pausedText.SetActive(false);
                GameManager.Instance.ResumeGame();
                _isTimeStopped = false;
            }
        }
        private bool _wasFreeCameraLastFrame = false;
        private bool isFreeCameraActive = false;
        private void HandleFreeCamera()
        {
            if (_isMenuOpen) return;

            bool isFreeCameraThisFrame = _input.freeCamera;
            bool isKeyDown = !_wasFreeCameraLastFrame && isFreeCameraThisFrame;
            _wasFreeCameraLastFrame = isFreeCameraThisFrame;
            if (isKeyDown && !isFreeCameraActive)
            {
                freeCameraText.SetActive(true);
                freeCamera.gameObject.SetActive(true);
                defaultCamera.gameObject.SetActive(false);
                _currentCameraTarget = freeCameraTarget;
                freeCameraTarget.position = defaultCamera.transform.position;
                uiCanvasGroup.alpha = 0f;
                canControlPlayer = false;
                isFreeCameraActive = true;
            }
            else if (isKeyDown && isFreeCameraActive)
            {
                freeCameraText.SetActive(false);
                freeCamera.gameObject.SetActive(false);
                defaultCamera.gameObject.SetActive(true);
                _currentCameraTarget = PlayerCameraTarget;
                uiCanvasGroup.alpha = 1f;
                canControlPlayer = true;
                isFreeCameraActive = false;
            }
            else if (isFreeCameraActive)
            {
                var speed = (_input.shoot ? 2f : 1f) * freeCameraSpeed;
                var moveDirection = (_input.move.x * freeCameraTarget.right + _input.move.y * freeCameraTarget.forward).normalized;
                freeCameraTarget.position += speed * Time.unscaledDeltaTime * moveDirection;
            }
        }


        private void CameraTargetRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplierPitch = IsCurrentDeviceMouse 
                    ? 1.0f 
                    : Time.unscaledDeltaTime * playerModel.RotationSpeedPitch;
                float deltaTimeMultiplierYaw = IsCurrentDeviceMouse
                    ? 1.0f
                    : Time.unscaledDeltaTime * playerModel.RotationSpeedYaw;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplierYaw;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplierPitch;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, CameraBottomClamp, CameraTopClamp);

            // Cinemachine will follow this target
            _currentCameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            float targetSpeed = 0f;
            if (_input.move.magnitude > 0f)
            {
                targetSpeed = _isSprinting
                    ? playerModel.SprintSpeed
                    : playerModel.MoveSpeed;
            }

            float currentHorizontalSpeed = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            if (currentHorizontalSpeed < targetSpeed - speedOffset
                || currentHorizontalSpeed > targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * playerModel.SpeedChangeRate
                );
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(
                _animationBlend,
                targetSpeed,
                Time.deltaTime * playerModel.SpeedChangeRate
            );
            if (_animationBlend < 0.01f) _animationBlend = 0f;
            if (_input.move.sqrMagnitude > 0f)
            {
                _lastInputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            }
            _targetYaw = Mathf.Rad2Deg * Mathf.Atan2(_lastInputDirection.x, _lastInputDirection.z)
                + _mainCamera.transform.rotation.eulerAngles.y;
            _lookYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetYaw,
                ref _rotationVelocity,
                playerModel.RotationSmoothTime * Time.unscaledDeltaTime
            );
            if (_input.move.magnitude > 0f)
            {
                armature.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
            }
            Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;
            var moveVelocity = Vector3.ProjectOnPlane(lookDirection, groundNormal) * _speed;
            var verticalVelocity = new Vector3(0.0f, _verticalVelocity, 0.0f);
            var moveDirection = (verticalVelocity + moveVelocity);
            if (_input.move.magnitude > 0f)
            {
                _rigidbody.velocity = moveDirection;
            }
            else
            {
                _rigidbody.velocity = new Vector3(0f, _verticalVelocity, 0f);
            }
            if (_hasAnimator)
            {
                animator.SetFloat(_animIDSpeed, _animationBlend);
                animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void MoveWhileAiming()
        {
            float targetSpeed = 0f;
            if (_input.move.magnitude > 0f)
            {
                targetSpeed = playerModel.MoveSpeed * 0.5f;
            }
            float currentHorizontalSpeed = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            if (currentHorizontalSpeed < targetSpeed - speedOffset
                || currentHorizontalSpeed > targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * playerModel.SpeedChangeRate
                );
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(
                _animationBlend,
                targetSpeed,
                Time.deltaTime * playerModel.SpeedChangeRate
            );
            if (_animationBlend < 0.01f) _animationBlend = 0f;
            if (_input.move.sqrMagnitude > 0f)
            {
                _lastInputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            }
            _targetYaw = Mathf.Rad2Deg * Mathf.Atan2(_lastInputDirection.x, _lastInputDirection.z)
                + _mainCamera.transform.rotation.eulerAngles.y;
            _lookYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetYaw,
                ref _rotationVelocity,
                playerModel.RotationSmoothTime * Time.unscaledDeltaTime
            );

            var cameraForward = defaultCamera.transform.forward;
            var armatureForward = (new Vector3(cameraForward.x, 0f, cameraForward.z)).normalized;
            armature.rotation = Quaternion.LookRotation(armatureForward, Vector3.up);
            Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;
            var moveVelocity = Vector3.ProjectOnPlane(lookDirection, groundNormal) * _speed;
            var verticalVelocity = new Vector3(0.0f, _verticalVelocity, 0.0f);
            var moveDirection = (verticalVelocity + moveVelocity);
            if (_input.move.magnitude > 0f)
            {
                _rigidbody.velocity = moveDirection;
            }
            else
            {
                _rigidbody.velocity = new Vector3(0f, _verticalVelocity, 0f);
            }
            if (_hasAnimator)
            {
                animator.SetFloat(_animIDSpeed, _animationBlend);
                animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        // TODO: fix dodge
        private void HandleDodgeRoll()
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
            _speed = Mathf.Lerp(
                playerModel.DodgeSpeed /2f,
                playerModel.DodgeSpeed,
                Mathf.Pow(speedFactor,4f)
            );
            _targetYaw = Mathf.Atan2(_lastInputDirection.x, _lastInputDirection.z) * Mathf.Rad2Deg
                         + _mainCamera.transform.eulerAngles.y;
            _lookYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetYaw,
                ref _rotationVelocity,
                playerModel.RotationSmoothTime * Time.unscaledDeltaTime
            );

            transform.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
            Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;

            _rigidbody.velocity = lookDirection.normalized * (_speed * Time.deltaTime)
                + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
        }

        private void JumpAndDodge()
        {
            var cooledDown = jumpCoolDownTime <= 0.0f && dodgeCoolDownTime <= 0.0f;
            if (_isGrounded)
            {
                _groundedCoyoteTime = 0f;
                if (_input.jump && cooledDown)
                {
                    DoJump();
                }
                else if (!_isDodging && _input.dodge && cooledDown)
                {
                    DoDodge();
                }
            }
            else if (_groundedCoyoteTime < groundedCoyoteDuration && cooledDown)
            {
                _groundedCoyoteTime += Time.deltaTime;
                if (_input.jump)
                {
                    DoJump();
                    _groundedCoyoteTime = groundedCoyoteDuration;
                }
                else if (!_isDodging && _input.dodge)
                {
                    DoDodge();
                    _groundedCoyoteTime = groundedCoyoteDuration;
                }
            }
            else
            {
                _input.jump = false;
                _input.dodge = false;
            }
        }

        private void HandleFallAndGravity()
        {
            var cooledDown = jumpCoolDownTime <= 0.0f && dodgeCoolDownTime <= 0.0f;
            if (_isGrounded)
            {
                _groundedCoyoteTime = 0f;
                fallTime = fallTimer;
                if (_hasAnimator)
                {
                    animator.SetBool(_animIDJump, false);
                    animator.SetBool(_animIDFreeFall, false);
                    animator.SetBool(_animIDDodge, false);
                }
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = 0f;
                }
                if (jumpCoolDownTime >= 0.0f)
                {
                    jumpCoolDownTime -= Time.deltaTime;
                }
                if (dodgeCoolDownTime >= 0.0f)
                {
                    _verticalVelocity = Mathf.Max(_verticalVelocity, playerModel.DodgeHeight);
                    dodgeCoolDownTime -= Time.deltaTime;
                }
            }
            else if (_groundedCoyoteTime >= groundedCoyoteDuration || !cooledDown)
            {
                if (_isDodging)
                {
                    dodgeCoolDownTime = dodgeCoolDownTimer;
                }
                else
                {
                    jumpCoolDownTime = jumpCoolDownTimer;
                }
                if (fallTime >= 0.0f)
                {
                    fallTime -= Time.deltaTime;
                }
                else if (_hasAnimator)
                {
                    animator.SetBool(_animIDFreeFall, true);
                }
            }
            if (_verticalVelocity < _terminalVelocity && _dodgeAscendTime <= 0f)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void DoJump()
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            _verticalVelocity = Mathf.Sqrt(playerModel.JumpHeight * -2f * Gravity);
            // update animator if using character
            if (_hasAnimator)
            {
                animator.SetBool(_animIDJump, true);
                MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
            }
        }

        private void DoDodge()
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            _verticalVelocity = Mathf.Sqrt(playerModel.DodgeHeight * -2f * Gravity);
            _isDodging = true;
            _dodgeAscendTime = DodgeAscendTimer;
            _dodgeLandTime = DodgeLandTimer;
            _groundedCoyoteTime = groundedCoyoteDuration;
            // update animator if using character
            if (_hasAnimator)
            {
                animator.SetBool(_animIDDodge, true);
                MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
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
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, FootstepAudioVolume);
            }
        }

        private void UpdateTimeManager(bool slowMoInput)
        {
            if (!_isSlowMoToggle && slowMoInput)
            {
                _isSlowMoStarted = !_isSlowMoStarted;
                if (_isSlowMoStarted)
                {
                    slowMoText.SetActive(true);
                    GameManager.Instance.timeManager.StartSlowMotion();
                }
                else
                {
                    slowMoText.SetActive(false);
                    GameManager.Instance.timeManager.StopSlowMotion();
                }
            }
            _isSlowMoToggle = slowMoInput;
        }

        public void Hit(int damage, Vector3 hitDirection)
        {
            if (_isDying)
            {
                return;
            }
            playerEventController.UpdateCurrentHealth(playerModel.CurrentHealth - damage);
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
            if (playerModel.CurrentHealth <= 0 && !_isDying)
            {
                _isDying = true;
                Die(hitDirection);
            }
            else
            {
                animator.SetTrigger(_animIDHit);
            }
            cameraShake.Shake(damage);
            animateMesh.HitFlash();
        }

        private void Die(Vector3 hitDirection)
        {
            animator.SetFloat("XDieDirection", Vector3.Dot(-hitDirection.normalized, transform.right));
            animator.SetFloat("ZDieDirection", Vector3.Dot(-hitDirection.normalized, transform.forward));
            animator.SetTrigger("Die");
            Destroy(hittableManager.gameObject);
            SceneLoader.Instance.ReloadScene(5f);
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );
            _isGrounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (!_isGrounded)
            {
                currentGround = null;
                groundNormal = Vector3.up;
                return;
            }
            Ray groundRay = new Ray(
                spherePosition,
                Vector3.down
            );
            Debug.DrawRay(groundRay.origin, groundRay.direction * GroundedRadius, Color.red);
            // check if the ray hits the ground

            if (!Physics.Raycast(groundRay, out RaycastHit hit, 2f * GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                groundNormal = Vector3.up;
                _isGrounded = false;
                return;
            }

            var groundedObject = hit.transform;
            groundNormal = hit.normal;
            if (groundedObject == currentGround)
            {
                return;
            }
            currentGround = groundedObject;
            lastGroundPosition = currentGround.position;

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
                GroundedRadius
            );
        }
    }
}
