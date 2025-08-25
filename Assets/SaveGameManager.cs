using UnityEngine;
using F3PS;
using System;
using System.Collections.Generic;
using System.Linq;

public class SaveGameManager : MonoBehaviour
{

    public GameData GameData;
    public PlayerEventController PlayerEventController;
    private const string GAME_DATA = "GameData";
    private const string DEFAULT_GAME_DATA = "GameDataDefault";


    private void Awake()
    {
        PlayerEventController = new PlayerEventController();
    }

    private void OnEnable()
    {
        PlayerEventController.OnCurrentSpawnPointChanged += OnCurrentSpawnPointChanged;
    }

    private void OnDisable()
    {
        PlayerEventController.OnCurrentSpawnPointChanged -= OnCurrentSpawnPointChanged;
    }

    public GameData LoadCurrentGameData()
    {
        var gameDataJson = PlayerPrefs.GetString(GAME_DATA);
        GameData gameData = JsonUtility.FromJson<GameData>(gameDataJson);
        Debug.Log(gameDataJson);
        return gameData;
    }

    public bool HasSaveGameData()
    {
        return PlayerPrefs.HasKey(GAME_DATA) && !string.IsNullOrEmpty(PlayerPrefs.GetString(GAME_DATA));
    }

    public void ResetSaveGame()
    {
        var defaultGameDataJson = PlayerPrefs.GetString(DEFAULT_GAME_DATA);
        PlayerPrefs.SetString(GAME_DATA, defaultGameDataJson);
        PlayerPrefs.Save();
    }

    private void SaveGameData()
    {
        try
        {
            var gameDataJson = JsonUtility.ToJson(GameData);
            PlayerPrefs.SetString(GAME_DATA, gameDataJson);
            PlayerPrefs.Save();
            Debug.Log(gameDataJson);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogError("GameData seems corrupted");
            ResetSaveGame();
            return;
        }
    }

    private void SaveDefaultGameData()
    {
        var gameDataJson = JsonUtility.ToJson(GameData);
        PlayerPrefs.SetString(DEFAULT_GAME_DATA, gameDataJson);
        PlayerPrefs.Save();
    }

    private void OnCurrentSpawnPointChanged(int _)
    {
        SaveGameData();
    }

    public void InitializeSaveData()
    {
        Debug.Log("InitializeSaveData");
        if (gameObject.activeSelf)
        {
            Debug.Log("isActiveAndEnabled");
            if (HasSaveGameData())
            {
                Debug.Log("HasSaveGameData");
                /*
                ** Use Existing save game data.
                **/
                GameData = LoadCurrentGameData();
            }
            else
            {
                Debug.Log("HasNoSaveGameData");

                /*
                ** Save game data with values found in inspector and scene.
                **/
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
                switchesInstanceIds.AddRange(fillOnShotSwitches.Select(fillOnShotSwitch => fillOnShotSwitch.gameObject.GetInstanceID()));
                var standOnSwitches = FindObjectsByType<SwitchesController>(FindObjectsSortMode.InstanceID);
                switchesInstanceIds.AddRange(standOnSwitches.Select(standOnSwitch => standOnSwitch.gameObject.GetInstanceID()));
                GameData.RegisterAllSwitches(switchesInstanceIds.ToArray());

                SaveDefaultGameData();
                SaveGameData();
            }
        }
        else
        {
            Debug.Log("Not isActiveAndEnabled");
            /*
            ** Else save game data from Inspector is used.
            ** Just use values found in inspector and scene.
            */
        }
        /*
        ** In any case initialize data
        */

        PlayerEventController.InitializeData(GameData.PlayerData);
    }
}
