﻿using F3PS;
using Player;
using TimeBending;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [Header("Player")]

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        
        [Tooltip("The jump height of the player while dodging")]
        public float DodgeHeight = 1.2f;
        
        [Tooltip("The jump length of the player while dodging")]
        public float DodgeSpeed = 60f; 
        
        [Tooltip("The time it takes for the dodge speed to cool off")]
        public float DodgeTimer = 0.5f;
        private float _dodgeTime;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("The time it takes to dodge again after landing from a dodge")]
        public float DodgeCoolDownTimer = 0.25f;
        private float _dodgeCoolDownTime;

        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpCoolDownTimer = 0.50f;
        private float _jumpCoolDownTime;

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
        
        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetYaw = 0.0f;
        private float _lookYaw = 0.0f;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // animation IDs
        private readonly int _animIDSpeed = Animator.StringToHash("Speed");
        private readonly int _animIDGrounded = Animator.StringToHash("Grounded");
        private readonly int _animIDJump = Animator.StringToHash("Jump");
        private readonly int _animIDFreeFall = Animator.StringToHash("FreeFall");
        private readonly int _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        private readonly int _animIDDodge = Animator.StringToHash("Dodge");
        
        private Vector3 _lastInputDirection;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        [SerializeField] private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        [SerializeField] private StarterAssetsInputs _input;
        public StarterAssetsInputs Input => _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        public Extensions extensions;

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
            extensions.Init(_animator);
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            // reset our timeouts on start
            _jumpCoolDownTime = JumpCoolDownTimer;
            _fallTimeoutDelta = FallTimeout;
            _dodgeCoolDownTime = DodgeCoolDownTimer;
        }

        private void Update()
        {
            if (GameManager.Instance.IsGamePaused) return;
            
            extensions.OnUpdate(_input);
            
            if (GameManager.Instance.timeManager.IsPaused) return;
            _hasAnimator = TryGetComponent(out _animator);

            _animator.SetBool(_animIDGrounded, extensions.Grounded);
            JumpAndGravity();
            Move();
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
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.unscaledDeltaTime * extensions.RotationSpeed;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, CameraBottomClamp, CameraTopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            if (extensions.isDodging)
            {
                Dodge();
                return;
            }
            
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
            float targetSpeed = extensions.GetTargetSpeed(_input.move);

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
            _lastInputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            _targetYaw = _mainCamera.transform.eulerAngles.y
                         + Mathf.Rad2Deg * Mathf.Atan2(_lastInputDirection.x, _lastInputDirection.z);
            _lookYaw = extensions.GetLookYaw(transform, _targetYaw, _cinemachineTargetYaw);
            
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
            _dodgeTime -= Time.deltaTime;
            _dodgeTime = Mathf.Max(_dodgeTime, 0f);
            _speed = Mathf.Lerp(0f, DodgeSpeed, _dodgeTime/DodgeTimer) ;
            _targetYaw = Mathf.Atan2(_lastInputDirection.x, _lastInputDirection.z) * Mathf.Rad2Deg
                         + _mainCamera.transform.eulerAngles.y;
            _lookYaw = extensions.GetLookYaw(transform, _targetYaw, _cinemachineTargetYaw);
            
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
            if (extensions.Grounded)
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
                else if (_input.dodge && _jumpCoolDownTime <= 0.0f && _dodgeCoolDownTime <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(DodgeHeight * -2f * Gravity);
                    extensions.isDodging = true;
                    _dodgeTime = DodgeTimer;
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
                    extensions.isDodging = false;
                    _dodgeCoolDownTime -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                if (extensions.isDodging)
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
            if (_verticalVelocity < _terminalVelocity)
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
    }
}
