using System;
using System.Collections.Generic;
using System.Linq;
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

        public GameData GameData;
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
            
            InitializeSaveData();

#if !ENABLE_INPUT_SYSTEM || !STARTER_ASSETS_PACKAGES_CHECKED
            LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
        }

        private void InitializeSaveData()
        {
            if (saveGameManager.isActiveAndEnabled && !saveGameManager.HasGameSaveData())
            /*
            ** Save game data with values found in inspector and scene.
            **/
            {
                // register enemies from scene
                var enemiesInstanceIds = new List<int>();
                var scorpions = FindObjectsByType<ScorpionController>(FindObjectsSortMode.InstanceID);
                var yaggiStandards = FindObjectsByType<YaggiStandardController>(FindObjectsSortMode.InstanceID);
                var yaggiSpitters = FindObjectsByType<YaggiSpitterController>(FindObjectsSortMode.InstanceID);
                var yaggiShieldSpitters = FindObjectsByType<YaggiShieldSpitterController>(FindObjectsSortMode.InstanceID);
                enemiesInstanceIds.AddRange(scorpions.Select(scorpion => scorpion.gameObject.GetInstanceID()));
                enemiesInstanceIds.AddRange(yaggiStandards.Select(yaggiStandard => yaggiStandard.gameObject.GetInstanceID()));
                enemiesInstanceIds.AddRange(yaggiSpitters.Select(yaggiSpitter => yaggiSpitter.gameObject.GetInstanceID()));
                enemiesInstanceIds.AddRange(yaggiShieldSpitters.Select(yaggiShieldSpitter => yaggiShieldSpitter.gameObject.GetInstanceID()));
                GameData.RegisterAllEnemies(enemiesInstanceIds.ToArray());

                // register switches from scene
                var switchesInstanceIds = new List<int>();
                var fillOnShotSwitches = FindObjectsByType<FillOnShot>(FindObjectsSortMode.InstanceID);
                var standOnSwitches = FindObjectsByType<SwitchesController>(FindObjectsSortMode.InstanceID);
                switchesInstanceIds.AddRange(fillOnShotSwitches.Select(fillOnShotSwitch => fillOnShotSwitch.gameObject.GetInstanceID()));
                switchesInstanceIds.AddRange(standOnSwitches.Select(standOnSwitch => standOnSwitch.gameObject.GetInstanceID()));
                GameData.RegisterAllSwitches(switchesInstanceIds.ToArray());

                saveGameManager.SaveDefaultGameData(GameData);
                saveGameManager.SaveGameData(GameData);
            }
            else if (saveGameManager.isActiveAndEnabled && saveGameManager.HasGameSaveData())
            /*
            ** Use Existing save game data.
            **/
            {
                GameData = saveGameManager.LoadCurrentGameData();
            }
            /*
            ** Else dont use save game data.
            ** Just use values found in inspector and scene.
            */
            GameData.PlayerEventController.InitializeData(GameData.PlayerData);
        }

        private void Start()
        {
            Application.targetFrameRate = _fps;
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
            InitializeSaveData();
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
