using DG.Tweening;
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
        public InGameMenu inGameMenu;

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
            DOTween.Init();
        }

        private void Start()
        {
            Application.targetFrameRate = _fps;
        }

        public void PauseGame()
        {
            timeManager.PauseTime();
            _isGamePaused = true;
        }

        public void ResumeGame()
        {
            timeManager.ResumeTime();
            _isGamePaused = false;
        }
    }
}
