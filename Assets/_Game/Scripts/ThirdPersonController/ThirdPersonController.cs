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
        public LayerMask GroundLayers;
        public LayerMask SolidGroundLayers;
        public Transform currentGround;
        public Vector3 groundNormal;
        public Vector3 groundHitPointLocal;
        public Vector3 groundHitPointWorld;
        public Vector3 lastGroundPosition;

        [Header("Gravity & Ground")]
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

            if (!GameManager.Instance.inputs.canControlPlayer) return;
            if (timeManager.isPaused) return;
            if (_isDying) return;

            animator.SetBool(_animIDGrounded, _isGrounded);

            skillManager.OnUpdate();
            attackManager.OnUpdate();

            HandleSprint();
        }

        private void HandleSprint()
        {
            if (!_data.UnlockedPassiveSkills.Contains(PassiveSkills.Sprint))
            {
                _isSprinting = false;
                return;
            }
            var sprintStaminaDepletion = GameManager.Instance.PlayerData.SprintDepletionRate * Time.deltaTime;
            if (staminaManager.IsRecoveringStamina)
            {
                _isSprinting = false;
            }
            else if (GameManager.Instance.inputs.sprint)
            {
                staminaManager.Deplete(sprintStaminaDepletion);
                _isSprinting = true;
            }
        }

        private void FixedUpdate()
        {
            if (!GameManager.Instance.inputs.canControlPlayer) return;
            if (timeManager.isPaused) return;
            if (_isDying) return;

            HandlePlatformTransform();
            GroundedCheck();
            HandleFallAndGravity();

            skillManager.OnFixedUpdate();
            attackManager.OnFixedUpdate();

            if (!skillManager.IsAiming())
            {
                JumpAndDodge();
            }
            
            Move(skillManager.IsAiming());
        }

        private void LateUpdate()
        {
            if (!GameManager.Instance.isMenuOpen && !skillManager.thotMindController.isRotatingObjectThisFrame)
            {
                cameraSettings.CameraTargetRotation();
            }
            if (GameManager.Instance.inputs.canControlPlayer && !timeManager.isPaused && !_isDying)
            {
                skillManager.OnLateUpdate();
            }
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
            currentGround = groundedObject;
            groundNormal = hit.normal;
            groundHitPointWorld = hit.point;
            groundHitPointLocal = groundedObject.InverseTransformPoint(groundHitPointWorld);
            lastGroundPosition = groundedObject.position;

            if (groundedObject.gameObject.IsInLayerMask(SolidGroundLayers))
            {
                checkGroundTime += Time.fixedDeltaTime;
                if (checkGroundTime >= checkGroundTimer)
                {
                    checkGroundTime = 0f;
                    beforeLastValidGroundPosition = lastValidGroundPosition;
                    lastValidGroundPosition = groundHitPointWorld;
                }
            }
            else
            {
                checkGroundTime = 0f;
            }
        }

        public void ResetToLastGroundPosition()
        {
            transform.position = beforeLastValidGroundPosition + Vector3.up;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        private void HandlePlatformTransform()
        {
            if (!_isGrounded)
            {
                return;
            }

            var currentGroundHitPointWorld = currentGround.TransformPoint(groundHitPointLocal);
            var groundMovedDirection = currentGroundHitPointWorld - groundHitPointWorld;
            _rigidbody.MovePosition(_rigidbody.position + groundMovedDirection);
            groundHitPointWorld = currentGroundHitPointWorld;
            groundHitPointLocal = currentGround.InverseTransformPoint(groundHitPointWorld);
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
            if (GameManager.Instance.inputs.move.magnitude > 0f)
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
            float inputMagnitude = GameManager.Instance.inputs.analogMovement ? GameManager.Instance.inputs.move.magnitude : 1f;
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
            if (GameManager.Instance.inputs.move.sqrMagnitude > 0f)
            {
                _lastInputDirection = new Vector3(GameManager.Instance.inputs.move.x, 0.0f, GameManager.Instance.inputs.move.y).normalized;
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
            else if(GameManager.Instance.inputs.move.magnitude > 0f)
            {
                armature.rotation = Quaternion.Euler(0.0f, _lookYaw, 0.0f);
            }

            var verticalVelocity = new Vector3(0.0f, currentVerticalSpeed, 0.0f);
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


            if (_isGrounded && currentGround != null)
            {
                groundHitPointWorld = transform.position;
                groundHitPointLocal = currentGround.InverseTransformPoint(groundHitPointWorld);
            }
        }

        private void JumpAndDodge()
        {
            var jumpInput = GameManager.Instance.inputs.jump;
            var jump = jumpInput && !wasJumpPressedLastFrame;
            wasJumpPressedLastFrame = jumpInput;

            var dodgeInput = GameManager.Instance.inputs.dodge;
            var dodge = dodgeInput && !wasDodgePressedLastFrame;
            wasDodgePressedLastFrame = dodgeInput;
            if (!_isGrounded && groundedCoyoteDuration <= _groundedCoyoteTime)
            {
                return;
            }

            if (jump && jumpCoolDownTime <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                currentVerticalSpeed = Mathf.Sqrt(_data.JumpHeight * -2f * Gravity);
                MasterAudio.PlaySound3DAtTransformAndForget("Player_jump", transform);
                _groundedCoyoteTime = groundedCoyoteDuration;

                landingPlane.SetActive(true);
                UpdateLandingPlane();

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
            var jumpInput = GameManager.Instance.inputs.jump;
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
            else if (_data.UnlockedPassiveSkills.Contains(PassiveSkills.Glide) && jumpInput && isAscending)
            {
                UpdateLandingPlane();
                ascendTime += Time.deltaTime;
                if (ascendTime >= _data.AscendDuration)
                {
                    isAscending = false;
                    isGliding = true;
                    ascendTime = 0f;
                }
                var maximumJumpSpeed = Mathf.Sqrt(_data.JumpHeight * -2f * Gravity);
                var easing = Helper.Easing.EaseInQuad(ascendTime / _data.AscendDuration);
                currentVerticalSpeed = Mathf.Lerp(maximumJumpSpeed, 0f, easing);
            }
            else if (_data.UnlockedPassiveSkills.Contains(PassiveSkills.Glide) && jumpInput && isGliding)
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
