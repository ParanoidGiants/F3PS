using UnityEngine;
using F3PS;
using System;

public class SaveGameManager : MonoBehaviour
{
    private const string GAME_DATA = "GameData";
    private const string DEFAULT_GAME_DATA = "GameDataDefault";

    public void SavePlayerData(PlayerData playerData)
    {
        try
        {
            var playerDataJson = JsonUtility.ToJson(playerData);
            PlayerPrefs.SetString(GAME_DATA, playerDataJson);
            PlayerPrefs.Save();
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

    public PlayerData LoadCurrentPlayerData()
    {
        var playerDataJson = PlayerPrefs.GetString(GAME_DATA);
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(playerDataJson);
        Debug.Log(playerDataJson);
        return playerData;
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
    }

    public void SaveDefaultPlayerData(PlayerData playerData)
    {
        var playerDataJson = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(DEFAULT_GAME_DATA, playerDataJson);
        PlayerPrefs.Save();
    }
}
