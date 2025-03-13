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
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Application.targetFrameRate = _fps;
            inputs = FindObjectOfType<ThirdPersonController>().Input;
            timeManager = FindObjectOfType<TimeManager>();
        }

        public void PauseGame()
        {
            GameManager.Instance.timeManager.PauseTime();
            _isGamePaused = true;
        }

        public void ResumeGame()
        {
            GameManager.Instance.timeManager.ResumeTime();
            _isGamePaused = false;
        }
    }
}
