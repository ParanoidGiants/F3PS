using UnityEngine;
using F3PS;
using System;

public class SaveGameManager : MonoBehaviour
{
    private const string GAME_DATA = "GameData";
    private const string DEFAULT_GAME_DATA = "GameDataDefault";

    private void Start()
    {
        GameManager.Instance.PlayerEventController.OnCurrentSpawnPointChanged += SavePlayerData;
    }

    public void SavePlayerData(int spawnPointIndex)
    {
        try
        {
            var playerDataJson = JsonUtility.ToJson(GameManager.Instance.PlayerData);
            PlayerPrefs.SetString(GAME_DATA, playerDataJson);
            PlayerPrefs.Save();
            Debug.Log($"Saved Game Data at SpawnPoint {spawnPointIndex}");
            Debug.Log(playerDataJson);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogError("GameData seems corrupted");
            ResetSaveGame();
            return;
        }
    }

    public void LoadCurrentPlayerData()
    {
        Debug.Log("Loading GameData...");
        var playerDataJson = PlayerPrefs.GetString(GAME_DATA);
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(playerDataJson);
        GameManager.Instance.PlayerData = playerData;
        Debug.Log("Loaded GameData");
        Debug.Log(playerDataJson);
    }

    public bool HasGameSaveData()
    {
        return PlayerPrefs.HasKey(GAME_DATA) && !string.IsNullOrEmpty(PlayerPrefs.GetString(GAME_DATA));
    }

    public void ResetSaveGame()
    {
        var defaultGameDataJson = PlayerPrefs.GetString(DEFAULT_GAME_DATA);
        PlayerPrefs.SetString(GAME_DATA, defaultGameDataJson);
        PlayerPrefs.Save();
        Debug.Log("Resetting Save State");
    }

    public void SaveInitialPlayerData()
    {
        Debug.Log("Overwriting GameData with default values...");
        SavePlayerData(0);
        PlayerData playerData = GameManager.Instance.PlayerData;
        var playerDataJson = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(DEFAULT_GAME_DATA, playerDataJson);
        PlayerPrefs.Save();
    }
}
