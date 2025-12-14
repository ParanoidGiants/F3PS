using UnityEngine;
using F3PS;
using System;
using System.Collections.Generic;
using System.Linq;

public class SaveGameManager : MonoBehaviour
{

    public GameData GameData;
    public PlayerEventController PlayerEventController;
    public SwitchEventController SwitchEventController;
    public DoorEventController DoorEventController;
    public EnemyEventController EnemyEventController;

    private const string GAME_DATA = "GameData";
    private const string DEFAULT_GAME_DATA = "GameDataDefault";


    private void Awake()
    {
        SaveDefaultGameData();
        PlayerEventController = new PlayerEventController();
        SwitchEventController = new SwitchEventController();
        DoorEventController = new DoorEventController();
        EnemyEventController = new EnemyEventController();
    }

    private void OnEnable()
    {
        PlayerEventController.OnCurrentSpawnPointChanged += OnCurrentSpawnPointChanged;
    }

    private void OnDisable()
    {
        PlayerEventController.OnCurrentSpawnPointChanged -= OnCurrentSpawnPointChanged;
    }

    private void InitializeEventControllers()
    {
        PlayerEventController.InitializeData(GameData.PlayerData);
        SwitchEventController.InitializeData(GameData.SwitchesData);
        DoorEventController.InitializeData(GameData.DoorsData);
        EnemyEventController.InitializeData(GameData.EnemiesData);
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
        if (!gameObject.activeSelf)
        {
            /*
            ** Save game data from Inspector is used.
            ** Just use values found in inspector and scene.
            */
            Debug.Log("Not isActiveAndEnabled");
            InitializeEventControllers();
            return;
        }

        Debug.Log("isActiveAndEnabled");
        if (HasSaveGameData())
        {
            /*
            ** Use Existing save game data.
            **/
            Debug.Log("HasSaveGameData");
            GameData = LoadCurrentGameData();
            InitializeEventControllers();
            return;
        }

        /*
        ** Create Save game data with values found in inspector and scene.
        **/
        try
        {
            Debug.Log("HasNoSaveGameData");
            CreateDefaultGameData();
            InitializeEventControllers();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogError("Failed to create default game data, using values from inspector and scene.");
            return;
        }
    }

    private void CreateDefaultGameData()
    {
        // register enemies from scene
        var enemyGroupNames = new List<string>();
        var enemyGroups = FindObjectsByType<RegisterEnemy>(FindObjectsSortMode.InstanceID);
        enemyGroupNames.AddRange(enemyGroups.Select(enemyGroup => enemyGroup.gameObject.name));
        if (enemyGroupNames.Count != enemyGroupNames.Distinct().Count())
        {
            Debug.LogError("Every enemy group needs to have a unique name");
            throw new Exception("Duplicate enemy groups found");
        }
        GameData.EnemiesData.Initialize(enemyGroupNames.ToArray());

        // register switches from scene
        GameData.SwitchesData = new SwitchesData();
        var switchesIds = new List<string>();
        var fillOnShotSwitches = FindObjectsByType<FillOnShot>(FindObjectsSortMode.InstanceID);
        switchesIds.AddRange(fillOnShotSwitches.Select(fillOnShotSwitch => fillOnShotSwitch.gameObject.name));
        if (switchesIds.Count != switchesIds.Distinct().Count())
        {
            Debug.LogError("Every FillOnShot switch needs to have a unique name");
            throw new Exception("Duplicate FillOnShot switches found");
        }
        // var standOnSwitches = FindObjectsByType<SwitchesController>(FindObjectsSortMode.InstanceID);
        // switchesIds.AddRange(standOnSwitches.Select(standOnSwitch => standOnSwitch.gameObject.GetInstanceID()));
        GameData.SwitchesData.Initialize(switchesIds.ToArray());

        // register doors from scene
        GameData.DoorsData = new DoorsData();
        var doorsIds = new List<string>();
        var doors = FindObjectsByType<DoorController>(FindObjectsSortMode.InstanceID);
        doorsIds.AddRange(doors.Select(door => door.gameObject.name));
        // are there any duplicate doors?
        if (doorsIds.Count != doorsIds.Distinct().Count())
        {
            Debug.LogError("Every DoorController needs to have a unique name");
            throw new Exception("Duplicate doors found");
        }
        GameData.DoorsData.Initialize(doorsIds.ToArray());

        SaveDefaultGameData();
        SaveGameData();
    }
}