using DG.Tweening;
using StarterAssets;
using TimeBending;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace F3PS
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;
        public GameData GameData => saveGameManager.GameData;
        public SaveGameManager saveGameManager;

        public StarterAssetsInputs inputs;
        public bool IsCurrentDeviceMouse
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
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        [SerializeField] private int _fps = 60;
        public bool isMenuOpen;

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
            _playerInput = inputs.GetComponent<PlayerInput>();

#if !ENABLE_INPUT_SYSTEM || !STARTER_ASSETS_PACKAGES_CHECKED
            LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
        }

        private void Start()
        {
            Application.targetFrameRate = _fps;
            saveGameManager.InitializeSaveData();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            inputs.canControlPlayer = true;
            inputs.SetCursorLockedState(true);
            isMenuOpen = false;
            saveGameManager.InitializeSaveData();
        }

        public void ResumeGameAfterMenuClosed()
        {
            FindFirstObjectByType<TimeManager>().ResumeTime();
            inputs.SetCursorLockedState(true);
            inputs.canControlPlayer = true;
            isMenuOpen = false;
        }

        public void StopGameAfterMenuOpened()
        {
            FindFirstObjectByType<TimeManager>().PauseTime();
            inputs.SetCursorLockedState(false);
            inputs.canControlPlayer = false;
            isMenuOpen = true;
        }

        public void ActivateFreeCamera()
        {
            FindFirstObjectByType<DebugUIController>().ShowFreeCameraText();
            FindFirstObjectByType<HUDController>().canvasGroup.alpha = 0f;
            inputs.canControlPlayer = false;
        }

        internal void DeactivateFreeCamera()
        {
            FindFirstObjectByType<DebugUIController>().HideFreeCameraText();
            FindFirstObjectByType<HUDController>().canvasGroup.alpha = 1f;
            inputs.canControlPlayer = true;
        }

        public void PauseTime()
        {
            FindFirstObjectByType<DebugUIController>().ShowPauseText();
        }

        internal void ResumeTime()
        {
            FindFirstObjectByType<DebugUIController>().HidePauseText();
        }

        internal void StartSlowMotion()
        {
            FindFirstObjectByType<DebugUIController>().ShowSlowMoText();
        }

        internal void StopSlowMotion()
        {
            FindFirstObjectByType<DebugUIController>().HideSlowMoText();
        }
    }
}
