using F3PS;
using UnityEngine;
using TimeBending;



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
        private PlayerData _playerModel;
        private PlayerEventController _playerEventController;

        [Space(20)]
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
        [Header("Settings")]
        [Header("Camera Settings")]
        public ThirdPersonCameraSettings cameraSettings;
        public float jumpCoolDownTimer = 0.25f;
        public float jumpCoolDownTime;
        public float dodgeCoolDownTimer = 0.25f;
        public float dodgeCoolDownTime;
        public float fallTimer = 0.15f;
        public float fallTime;

        [Space(10)]
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Space(10)]
        [Header("Stair Climbing")]
        [Tooltip("The maximum height of a step the character can climb.")]
        public float MaxStepHeight = 0.4f;

        [Tooltip("How far in front of the player to check for steps.")]
        public float StepCheckDistance = 0.4f;

        [Tooltip("How fast the player will move up the step. This is an upward force.")]
        public float StepUpForce = 10f;
        public bool isStairsClimbing = false;
        public bool isStairAtLower = false;
        public bool isStairAtUpper = false;

        [Space(10)]
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
        [SerializeField] private bool _isDying;
        [SerializeField] private float _rotationVelocity;
        [SerializeField] private float _speed;
        [SerializeField] private float _animationBlend;
        [SerializeField] private float _targetYaw;
        [SerializeField] private float _lookYaw;
        [SerializeField] private float _verticalVelocity;
        [SerializeField] private Vector3 _lastInputDirection;

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
        private const float _terminalVelocity = 53.0f;

        public Vector3 lastValidGroundPosition = Vector3.zero;
        public Vector3 beforeLastValidGroundPosition = Vector3.zero;
        public float checkGroundTimer = 1f;
        public float checkGroundTime = 0f;

        private Rigidbody _rigidbody;


        public bool IsGrounded => _isGrounded;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _playerModel = GameManager.Instance.PlayerData;
            _playerEventController = GameManager.Instance.PlayerEventController;
        }

        private void Start()
        {
            cameraSettings.Start();
            // reset our timeouts on start
            jumpCoolDownTime = jumpCoolDownTimer;
            fallTime = fallTimer;
            dodgeCoolDownTime = dodgeCoolDownTimer;
            skillManager.Init();
            attackManager.Init();
        }
        private void Update()
        {

            if (GameManager.Instance.isMenuOpen) return;
            
            cameraSettings.HandleFreeCamera();

            if (!GameManager.Instance.inputs.canControlPlayer) return;
            if (_isDying) return;
            if (timeManager.isPaused) return;

            animator.SetBool(_animIDGrounded, _isGrounded);

            skillManager.OnUpdate();
            attackManager.OnUpdate();

            
            _isSprinting = staminaManager.Sprint();
            HandlePlatformTransform();
        }

        private void FixedUpdate()
        {
            if (!GameManager.Instance.inputs.canControlPlayer) return;
            if (GameManager.Instance.isGamePaused) return;
            if (timeManager.isPaused) return;
            if (_isDying) return;


            GroundedCheck();
            HandleFallAndGravity();

            skillManager.OnFixedUpdate();
            attackManager.OnFixedUpdate();

            if (!skillManager.IsAiming())
            {
                JumpAndDodge();
                HandleDodgeRoll();
            }
            Move(skillManager.IsAiming());
            HandleClimbingStairs();
        }

        private void LateUpdate()
        {
            if (GameManager.Instance.isMenuOpen)
                return;

            if (skillManager.telekinesisController.isRotatingObjectThisFrame)
                return;

            cameraSettings.CameraTargetRotation();
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

        private void Move(bool isAiming)
        {
            float targetSpeed = 0f;
            if (GameManager.Instance.inputs.move.magnitude > 0f)
            {
                if (isAiming)
                {
                    targetSpeed = _playerModel.AimSpeed;
                }
                else if (_isSprinting)
                {
                    targetSpeed = _playerModel.SprintSpeed;
                }
                else
                {
                    targetSpeed = _playerModel.MoveSpeed;
                }
            }

            float currentHorizontalSpeed = new Vector3(_rigidbody.linearVelocity.x, 0.0f, _rigidbody.linearVelocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = GameManager.Instance.inputs.analogMovement ? GameManager.Instance.inputs.move.magnitude : 1f;
            if (currentHorizontalSpeed < targetSpeed - speedOffset
                || currentHorizontalSpeed > targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * _playerModel.SpeedChangeRate
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
                Time.deltaTime * _playerModel.SpeedChangeRate
            );
            if (_animationBlend < 0.01f) _animationBlend = 0f;
            if (GameManager.Instance.inputs.move.sqrMagnitude > 0f)
            {
                _lastInputDirection = new Vector3(GameManager.Instance.inputs.move.x, 0.0f, GameManager.Instance.inputs.move.y).normalized;
            }
            _targetYaw = cameraSettings.GetTargetYawFromInputDirection(_lastInputDirection);
            _lookYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetYaw,
                ref _rotationVelocity,
                _playerModel.RotationSmoothTime * Time.unscaledDeltaTime
            );
            if (GameManager.Instance.inputs.move.magnitude > 0f)
            {
                if (isAiming)
                {
                    var cameraForward = cameraSettings.defaultCamera.transform.forward;
                    var armatureForward = (new Vector3(cameraForward.x, 0f, cameraForward.z)).normalized;
                    armature.rotation = Quaternion.LookRotation(armatureForward, Vector3.up);
                }
                else
                {
                    armature.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
                }
            }

            var verticalVelocity = new Vector3(0.0f, _verticalVelocity, 0.0f);
            if (GameManager.Instance.inputs.move.magnitude > 0f)
            {
                Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;
                var moveVelocity = Vector3.ProjectOnPlane(lookDirection, groundNormal) * _speed;
                _rigidbody.linearVelocity = (verticalVelocity + moveVelocity);
            }
            else
            {
                _rigidbody.linearVelocity = verticalVelocity;
            }

            animator.SetFloat(_animIDSpeed, _animationBlend);
            animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

        private void HandleClimbingStairs()
        {
            if (GameManager.Instance.inputs.move == Vector2.zero)
            {
                isStairsClimbing = false;
                return;
            }

            Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;
            var moveVelocity = Vector3.ProjectOnPlane(lookDirection, groundNormal) * _speed;
            Vector3 moveDirection = new Vector3(GameManager.Instance.inputs.move.x, 0, GameManager.Instance.inputs.move.y);
            Vector3 forwardDirection = moveVelocity.normalized;

            Vector3 lowerRayStart = transform.position + Vector3.up * 0.05f;

            RaycastHit lowerHit;
            Debug.DrawLine(lowerRayStart, lowerRayStart + forwardDirection * StepCheckDistance, Color.blue);
            isStairAtLower = Physics.Raycast(lowerRayStart, forwardDirection, out lowerHit, StepCheckDistance, GroundLayers);

            Vector3 upperRayStart = lowerRayStart + Vector3.up * (MaxStepHeight - 0.05f);
            Debug.DrawLine(upperRayStart, upperRayStart + forwardDirection * StepCheckDistance, Color.green);
            isStairAtUpper = Physics.Raycast(upperRayStart, forwardDirection, StepCheckDistance, GroundLayers);

            if (!isStairsClimbing && isStairAtLower && !isStairAtUpper)
            {
                isStairsClimbing = true;
            }
            else if (isStairsClimbing && !isStairAtLower)
            {
                isStairsClimbing = false;
            }
            else if (isStairsClimbing)
            {
                _rigidbody.linearVelocity = new Vector3(
                    _rigidbody.linearVelocity.x,
                    StepUpForce,
                    _rigidbody.linearVelocity.z
                );
            }
        }

        // TODO: fix dodge
        private void HandleDodgeRoll()
        {
            if (!_isDodging)
            {
                return;
            }

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
                _playerModel.DodgeSpeed /2f,
                _playerModel.DodgeSpeed,
                Mathf.Pow(speedFactor,4f)
            );
            _targetYaw = cameraSettings.GetTargetYawFromInputDirection(_lastInputDirection);
            _lookYaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                _targetYaw,
                ref _rotationVelocity,
                _playerModel.RotationSmoothTime * Time.unscaledDeltaTime
            );

            transform.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
            Vector3 lookDirection = Quaternion.Euler(0.0f, _targetYaw, 0.0f) * Vector3.forward;

            _rigidbody.linearVelocity = lookDirection.normalized * (_speed * Time.deltaTime)
                + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
        }

        private void JumpAndDodge()
        {
            var jumpInput = GameManager.Instance.inputs.jump;
            var dodgeInput = GameManager.Instance.inputs.dodge;
            var cooledDown = jumpCoolDownTime <= 0.0f && dodgeCoolDownTime <= 0.0f;
            if (_isGrounded)
            {
                _groundedCoyoteTime = 0f;
                if (jumpInput && cooledDown)
                {
                    DoJump();
                }
                else if (!_isDodging && dodgeInput && cooledDown)
                {
                    DoDodge();
                }
            }
            else if (_groundedCoyoteTime < groundedCoyoteDuration && cooledDown)
            {
                _groundedCoyoteTime += Time.deltaTime;
                if (jumpInput)
                {
                    DoJump();
                    _groundedCoyoteTime = groundedCoyoteDuration;
                }
                else if (!_isDodging && dodgeInput)
                {
                    DoDodge();
                    _groundedCoyoteTime = groundedCoyoteDuration;
                }
            }
        }

        private void HandleFallAndGravity()
        {
            var cooledDown = jumpCoolDownTime <= 0.0f && dodgeCoolDownTime <= 0.0f;
            if (_isGrounded)
            {
                _groundedCoyoteTime = 0f;
                fallTime = fallTimer;
                animator.SetBool(_animIDJump, false);
                animator.SetBool(_animIDFreeFall, false);
                animator.SetBool(_animIDDodge, false);

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
                    _verticalVelocity = Mathf.Max(_verticalVelocity, _playerModel.DodgeHeight);
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
                animator.SetBool(_animIDFreeFall, true);
            }
            if (_verticalVelocity < _terminalVelocity && _dodgeAscendTime <= 0f)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void DoJump()
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            _verticalVelocity = Mathf.Sqrt(_playerModel.JumpHeight * -2f * Gravity);
            animator.SetBool(_animIDJump, true);
            MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
        }

        private void DoDodge()
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            _verticalVelocity = Mathf.Sqrt(_playerModel.DodgeHeight * -2f * Gravity);
            _isDodging = true;
            _dodgeAscendTime = DodgeAscendTimer;
            _dodgeLandTime = DodgeLandTimer;
            _groundedCoyoteTime = groundedCoyoteDuration;

            animator.SetBool(_animIDDodge, true);
            MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
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
            _playerEventController.UpdateCurrentHealth(_playerModel.CurrentHealth - damage);
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
            if (_playerModel.CurrentHealth <= 0 && !_isDying)
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

        private void GroundedCheck()
        {
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
                checkGroundTime = 0f;
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
                checkGroundTime = 0f;
                return;
            }

            var groundedObject = hit.transform;
            groundNormal = hit.normal;
            if (groundedObject != currentGround)
            {
                currentGround = groundedObject;
                lastGroundPosition = currentGround.position;
            }

            checkGroundTime += Time.deltaTime;
            if (checkGroundTime >= checkGroundTimer)
            {
                beforeLastValidGroundPosition = lastValidGroundPosition;
                lastValidGroundPosition = transform.position;
                checkGroundTime = 0f;
            }
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

        public void ResetToLastGroundPosition()
        {
            transform.position = lastValidGroundPosition + Vector3.up;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        public void PauseGame()
        {

        }

        public void ResumeGame()
        {
            throw new System.NotImplementedException();
        }
    }
}
