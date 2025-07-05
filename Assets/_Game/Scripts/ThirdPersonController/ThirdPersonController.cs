using F3PS;
using UnityEngine;
using TimeBending;




#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using DarkTonic.MasterAudio;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{

    [RequireComponent(typeof(Rigidbody))]
    public class ThirdPersonController : MonoBehaviour
    {
        private StarterAssetsInputs Inputs => GameManager.Instance.inputs;
        [Header("References")]
        public StaminaManager staminaManager;
        public TimeManager timeManager;
        public SkillManager skillManager;
        public AttackManager attackManager;
        public AnimateMesh animateMesh;
        public HittableManager hittableManager;
        public Animator animator;
        public Transform armature;

        [Space(10)]
        [Header("Camera Settings")]
        public ThirdPersonCameraSettings cameraSettings;

        [Space(10)]
        [Header("Jump Settings")]
        public float jumpCoolDownTime;
        public GameObject landingPlane;
        public float ascendTime = 0f;
        public bool isAscending = false;
        public float glideTime = 0f;
        public bool isGliding = false;
        public bool wasJumpPressedLastFrame = false;
        public bool wasDodgePressedLastFrame = false;

        [Space(10)]
        [Header("Dodge Settings")]
        public float dodgeAscendTime;
        public float dodgeLandTime;
        public float dodgeCoolDownTime;
        public Vector3 dodgeDirection = Vector3.forward;

        [Space(10)]
        [Header("Stair Climbing")]
        public float MaxStepHeight = 0.4f;
        public float StepCheckDistance = 0.4f;
        public float StepUpForce = 10f;
        public bool isStairsClimbing = false;
        public bool isStairAtLower = false;
        public bool isStairAtUpper = false;

        [Header("Gravity & Ground")]
        public float Gravity = -15.0f;
        public float currentVerticalSpeed;
        public float maximumVerticalFallSpeed = 53.0f;
        public float groundedCoyoteDuration = 0.3f;
        public float _groundedCoyoteTime = 0f;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask SolidGroundLayers;
        public LayerMask GroundLayers;
        public Vector3 groundNormal;

        [Header("Fall of ground")]
        public Vector3 lastValidGroundPosition = Vector3.zero;
        public Vector3 beforeLastValidGroundPosition = Vector3.zero;
        public float checkGroundTimer = 1f;
        public float checkGroundTime = 0f;

        [Header("Watchers")]
        [SerializeField] private bool _isGrounded;
        [SerializeField] private bool _isSprinting;
        [SerializeField] private bool _isShooting;
        [SerializeField] private bool _isReloading;
        [SerializeField] private bool _isDodging;
        [SerializeField] private bool _isSlowMoToggle;
        [SerializeField] private bool _isSlowMoStarted;
        [SerializeField] private bool _isDying;
        [SerializeField] private float _rotationVelocity;
        [SerializeField] private float _speed;
        [SerializeField] private float _animationBlend;
        [SerializeField] private float _targetYaw;
        [SerializeField] private float _lookYaw;
        [SerializeField] private Vector3 _lastInputDirection;

        [Header("Audio")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        // animation IDs
        private readonly int _animIDSpeed = Animator.StringToHash("Speed");
        private readonly int _animIDGrounded = Animator.StringToHash("Grounded");
        private readonly int _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        private readonly int _animIDDodge = Animator.StringToHash("Dodge");
        private readonly int _animIDHit = Animator.StringToHash("Hit");
        private readonly int _animIDVerticalVelocity = Animator.StringToHash("VerticalVelocity");

        private Rigidbody _rigidbody;
        private PlayerData _data;
        private PlayerEventController _playerEventController;

        public bool IsGrounded => _isGrounded;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _data = GameManager.Instance.PlayerData;
            _playerEventController = GameManager.Instance.PlayerEventController;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            cameraSettings.Start();
            jumpCoolDownTime = _data.JumpCoolDownTimer;
            dodgeCoolDownTime = _data.DodgeCoolDownTimer;
            skillManager.Init();
            attackManager.Init();
        }
        private void Update()
        {
            if (GameManager.Instance.isMenuOpen) return;
            
            cameraSettings.HandleFreeCamera();

            if (!Inputs.canControlPlayer) return;
            if (timeManager.isPaused) return;
            if (_isDying) return;

            animator.SetBool(_animIDGrounded, _isGrounded);

            skillManager.OnUpdate();
            attackManager.OnUpdate();

            HandleSprintInput();
            HandleGlidingInput();
        }

        private void LateUpdate()
        {
            if (!GameManager.Instance.isMenuOpen && !skillManager.thotMindController.isRotatingObjectThisFrame)
            {
                cameraSettings.CameraTargetRotation();
            }
            if (Inputs.canControlPlayer && !timeManager.isPaused && !_isDying)
            {
                skillManager.OnLateUpdate();
            }
        }

        [Header("Platform Movement")]
        private Rigidbody _activePlatform;
        private Vector3 _platformPositionOffset;
        private Quaternion _platformRotationOffset;
        private Vector3 _lastPlatformPosition;
        private Quaternion _lastPlatformRotation;
        private Vector3 _platformVelocity;


        private void FixedUpdate()
        {
            if (!Inputs.canControlPlayer || timeManager.isPaused || _isDying) return;

            GroundedCheck();
            UpdatePlatformVelocity();
            HandleFallAndGravity();

            skillManager.OnFixedUpdate();
            attackManager.OnFixedUpdate();

            if (!skillManager.IsAiming())
            {
                JumpAndDodge();
            }

            Move(skillManager.IsAiming());
        }

        private void UpdatePlatformVelocity()
        {
            if (_activePlatform == null)
            {
                _platformVelocity = Vector3.zero;
                return;
            }

            // Calculate the platform's movement and rotation since the last frame
            var rotationDelta = _activePlatform.rotation * Quaternion.Inverse(_lastPlatformRotation);
            Vector3 positionDelta = _activePlatform.position - _lastPlatformPosition;

            // Calculate the rotational effect on our position offset
            Vector3 rotatedOffset = rotationDelta * _platformPositionOffset;

            // The total displacement caused by the platform's movement and rotation
            Vector3 totalDisplacement = positionDelta + (rotatedOffset - _platformPositionOffset);

            // Calculate the effective velocity of the platform at our position
            _platformVelocity = totalDisplacement / Time.fixedDeltaTime;

            // Update our stored position and rotation for the next frame's calculation
            _lastPlatformPosition = _activePlatform.position;
            _lastPlatformRotation = _activePlatform.rotation;
            // Continuously update the offset in case the player moves on the platform
            _platformPositionOffset = transform.position - _activePlatform.position;
            _platformRotationOffset = Quaternion.Euler(0, rotationDelta.eulerAngles.y, 0);
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);

            if (Physics.SphereCast(spherePosition + Vector3.up * 0.1f, GroundedRadius, Vector3.down, out RaycastHit hit, 0.2f, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                _isGrounded = true;
                groundNormal = hit.normal;

                Rigidbody newPlatform = hit.collider.GetComponentInParent<Rigidbody>();
                if (_activePlatform != newPlatform)
                {
                    SwitchActivePlatform(newPlatform);
                }

                if (_activePlatform != null)
                {
                    _platformPositionOffset = transform.position - _activePlatform.position;
                    _lastPlatformPosition = _activePlatform.position;
                    _lastPlatformRotation = _activePlatform.rotation;
                }
            }
            else
            {
                _isGrounded = false;
                groundNormal = Vector3.up;

                if (_activePlatform != null)
                {
                    SwitchActivePlatform(null);
                }
            }
        }

        private void SwitchActivePlatform(Rigidbody newPlatform)
        {
            _activePlatform = newPlatform;

            if (_activePlatform != null)
            {
                _platformPositionOffset = transform.position - _activePlatform.position;
                _lastPlatformPosition = _activePlatform.position;
                _lastPlatformRotation = _activePlatform.rotation;
            }
        }

        private void HandleGlidingInput()
        {
            if (!isGliding)
            {
                return;
            }

            var glideDepletionRate = _data.GlideDepletionRate * Time.deltaTime;
            if (staminaManager.IsRecoveringStamina)
            {
                isGliding = false;
            }
            else
            {
                staminaManager.Deplete(glideDepletionRate);
            }
        }

        private void HandleSprintInput()
        {
            var moving = Inputs.move != Vector2.zero;
            var sprint = Inputs.sprint;
            if (!sprint || !_data.UnlockedAbilities.Contains(Ability.Sprint))
            {
                _isSprinting = false;
                return;
            }
            var sprintStaminaDepletion = GameManager.Instance.PlayerData.SprintDepletionRate * Time.deltaTime;
            if (staminaManager.IsRecoveringStamina || !moving || !_isGrounded)
            {
                _isSprinting = false;
            }
            else
            {
                staminaManager.Deplete(sprintStaminaDepletion);
                _isSprinting = true;
            }
        }

        public void ResetToLastGroundPosition()
        {
            transform.position = beforeLastValidGroundPosition + Vector3.up;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        private void Move(bool isAiming)
        {
            if (_isDodging)
            {

                if (dodgeAscendTime >= _data.DodgeAscendTimer && dodgeLandTime >= _data.DodgeLandTimer)
                {
                    _isDodging = false;
                    return;
                }
                else if (dodgeAscendTime < _data.DodgeAscendTimer)
                {
                    dodgeAscendTime += Time.deltaTime;
                }
                else if (_isGrounded)
                {
                    dodgeAscendTime = _data.DodgeAscendTimer;
                    dodgeLandTime += Time.deltaTime;
                }
                else
                {
                    dodgeAscendTime = _data.DodgeAscendTimer;
                }

                var currentTime = dodgeAscendTime + dodgeLandTime;
                var totalTime = _data.DodgeAscendTimer + _data.DodgeLandTimer;
                var timeFactor = currentTime / totalTime;
                timeFactor = Mathf.Min(timeFactor, 1f);
                var speed = Mathf.Lerp(_data.DodgeSpeed, 0f, Mathf.Pow(timeFactor, 4f));
                _targetYaw = cameraSettings.GetTargetYawFromInputDirection(_lastInputDirection);
                _lookYaw = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetYaw,
                    ref _rotationVelocity,
                    _data.RotationSmoothTime * Time.unscaledDeltaTime
                );
                _rigidbody.linearVelocity = dodgeDirection * speed
                    + new Vector3(0.0f, currentVerticalSpeed, 0.0f);
                return;
            }

            float targetSpeed = 0f;
            if (Inputs.move.magnitude > 0f)
            {
                if (isAiming)
                {
                    targetSpeed = _data.AimSpeed;
                }
                else if (_isSprinting)
                {
                    targetSpeed = _data.SprintSpeed;
                }
                else
                {
                    targetSpeed = _data.MoveSpeed;
                }
            }

            float currentHorizontalSpeed = new Vector3(_rigidbody.linearVelocity.x, 0.0f, _rigidbody.linearVelocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = Inputs.analogMovement ? Inputs.move.magnitude : 1f;
            if (currentHorizontalSpeed < targetSpeed - speedOffset
                || currentHorizontalSpeed > targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * _data.SpeedChangeRate
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
                Time.deltaTime * _data.SpeedChangeRate
            );
            if (_animationBlend < 0.01f) _animationBlend = 0f;
            if (Inputs.move.sqrMagnitude > 0f)
            {
                _lastInputDirection = new Vector3(Inputs.move.x, 0.0f, Inputs.move.y).normalized;
            }
            _targetYaw = cameraSettings.GetTargetYawFromInputDirection(_lastInputDirection);
            _lookYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetYaw,
                ref _rotationVelocity,
                _data.RotationSmoothTime * Time.unscaledDeltaTime
            );
            
            if (isAiming)
            {
                var cameraForward = cameraSettings.defaultCamera.transform.forward;
                var armatureForward = (new Vector3(cameraForward.x, 0f, cameraForward.z)).normalized;
                armature.rotation = Quaternion.LookRotation(armatureForward, Vector3.up);
            }
            else if(Inputs.move.magnitude > 0f)
            {
                armature.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
            }

            var verticalVelocity = new Vector3(0.0f, currentVerticalSpeed, 0.0f);
            if (Inputs.move.magnitude > 0f)
            {
                Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;
                var moveVelocity = Vector3.ProjectOnPlane(lookDirection, groundNormal) * _speed;
                _rigidbody.linearVelocity = verticalVelocity + moveVelocity + _platformVelocity; ;
            }
            else
            {
                _rigidbody.linearVelocity = verticalVelocity + _platformVelocity; ;
            }

            animator.SetFloat(_animIDSpeed, _animationBlend);
            animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

        private void JumpAndDodge()
        {
            var jumpInput = Inputs.jump;
            var jump = jumpInput && !wasJumpPressedLastFrame;
            wasJumpPressedLastFrame = jumpInput;

            var dodgeInput = Inputs.dodge;
            var dodge = dodgeInput && !wasDodgePressedLastFrame;
            wasDodgePressedLastFrame = dodgeInput;
            if (isAscending 
                || isGliding
                || !_isGrounded && groundedCoyoteDuration <= _groundedCoyoteTime
            ) {
                return;
            }

            if (jump && jumpCoolDownTime <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                currentVerticalSpeed = Mathf.Sqrt(_data.JumpHeight * -2f * Gravity);
                MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
                _groundedCoyoteTime = groundedCoyoteDuration;

                isAscending = true;
                ascendTime = 0f;
            }
            else if (dodge && dodgeCoolDownTime <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                currentVerticalSpeed = Mathf.Sqrt(_data.DodgeHeight * -2f * Gravity);
                animator.SetBool(_animIDDodge, true);
                MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
                _groundedCoyoteTime = groundedCoyoteDuration;

                _targetYaw = cameraSettings.GetTargetYawFromInputDirection(_lastInputDirection);
                dodgeDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;
                _isDodging = true;
                dodgeAscendTime = 0f;
                dodgeLandTime = 0f;
            }
        }

        private void UpdateLandingPlane()
        {
            Physics.Raycast(
                new Ray(transform.position, Vector3.down),
                out RaycastHit hit,
                _data.LandingDepth,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );
            landingPlane.transform.position = hit.point + Vector3.up * 0.01f;
        }

        private void  HandleFallAndGravity()
        {
            var jumpInput = Inputs.jump;
            if (_isGrounded)
            {
                animator.SetBool(_animIDDodge, false);

                _groundedCoyoteTime = 0f;
                currentVerticalSpeed = 0f;

                if (jumpCoolDownTime >= 0.0f)
                {
                    jumpCoolDownTime -= Time.deltaTime;
                }
                if (dodgeCoolDownTime >= 0.0f)
                {
                    dodgeCoolDownTime -= Time.deltaTime;
                }
            }
            else if (_data.UnlockedAbilities.Contains(Ability.Glide) && jumpInput && isAscending)
            {
                UpdateLandingPlane();
                ascendTime += Time.deltaTime;
                if (ascendTime >= _data.AscendDuration)
                {
                    isAscending = false;
                    isGliding = true;
                    landingPlane.SetActive(true);
                    UpdateLandingPlane();
                    ascendTime = _data.AscendDuration;
                }
                var maximumJumpSpeed = Mathf.Sqrt(_data.AscendHeight * -2f * Gravity);
                var easing = Helper.Easing.EaseInQuad(ascendTime / _data.AscendDuration);
                easing = Mathf.Clamp01(easing);
                currentVerticalSpeed = Mathf.Lerp(maximumJumpSpeed, 0f, easing);
            }
            else if (_data.UnlockedAbilities.Contains(Ability.Glide) && jumpInput && isGliding)
            {
                UpdateLandingPlane();
                glideTime += Time.deltaTime;
                if (glideTime >= _data.GlideDuration)
                {
                    isGliding = false;
                    glideTime = 0f;
                }
                currentVerticalSpeed = 0f;
            }
            else
            {
                landingPlane.SetActive(false);
                isAscending = false;
                isGliding = false;
                ascendTime = 0f;
                glideTime = 0f;

                _groundedCoyoteTime += Time.deltaTime;
                currentVerticalSpeed += Gravity * Time.deltaTime;
                currentVerticalSpeed = Mathf.Max(currentVerticalSpeed, -maximumVerticalFallSpeed);
            }
            animator.SetFloat(_animIDVerticalVelocity, currentVerticalSpeed);
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

        public void Hit(int damage, Vector3 hitDirection)
        {
            if (_isDying)
            {
                return;
            }
            _playerEventController.UpdateCurrentHealth(_data.CurrentHealth - damage);
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
            if (_data.CurrentHealth <= 0 && !_isDying)
            {
                _isDying = true;
                Die(hitDirection);
            }
            else
            {
                animator.SetTrigger(_animIDHit);
            }
            cameraSettings.cameraShake.Shake(damage);
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
