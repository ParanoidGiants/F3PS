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
        public bool IsPaused { get; private set; }
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
        
        bool wasPausedLastFrame = false;
        private void Update()
        {
            // Handle pause game
            bool isPausedThisFrame = inputs.pause;
            bool isKeyDown = !wasPausedLastFrame && isPausedThisFrame;
            wasPausedLastFrame = isPausedThisFrame;
            if (isKeyDown && !IsPaused)
            {
                Debug.Log("PAUSE");
                timeManager.PauseTime();
                IsPaused = true;
            }
            else if (isKeyDown &&  IsPaused)
            {
                Debug.Log("RESUME");
                timeManager.ResumeTime();
                IsPaused = false;
            }
        }
    }
}
