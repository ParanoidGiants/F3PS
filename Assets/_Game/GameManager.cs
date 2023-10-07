using System;
using StarterAssets;
using TimeBending;
using UnityEngine;

namespace F3PS
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;
        
        public StarterAssetsInputs inputs;
        
        public TimeManager timeManager;
        [SerializeField] private bool _isGamePaused;
        public bool IsGamePaused => _isGamePaused;
        [SerializeField] private int _fps = 60;
        public int Fps => _fps;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this; 
            }
        }

        private void Start()
        {
            Application.targetFrameRate = _fps;
            inputs = FindObjectOfType<ThirdPersonController>().Input;
            timeManager = FindObjectOfType<TimeManager>();
        }
        
        private void Update()
        {
            HandlePauseGameSwitch();
        }

        private bool _wasPausedLastFrame = false;
        private void HandlePauseGameSwitch()
        {
            bool isPausedThisFrame = inputs.pause;
            bool isKeyDown = !_wasPausedLastFrame && isPausedThisFrame;
            _wasPausedLastFrame = isPausedThisFrame;
            if (isKeyDown && !_isGamePaused)
            {
                PauseTime();
                _isGamePaused = true;
            }
            else if (isKeyDown && _isGamePaused)
            {
                ResumeTime();
                _isGamePaused = false;
            }
        }

        public void ResumeTime()
        {
            Debug.Log("Try Resume");
            if (timeManager.IsPaused)
            {
                timeManager.ResumeTime();
            }
        }

        public void PauseTime()
        {
            Debug.Log("Try Pause");
            if (!timeManager.IsPaused)
            {
                timeManager.PauseTime();
            }
        }
    }
}
