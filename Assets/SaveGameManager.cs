using UnityEngine;
using F3PS;
using System;

public class SaveGameManager : MonoBehaviour
{
    private const string GAME_DATA = "GameData";
    private const string DEFAULT_GAME_DATA = "GameDataDefault";

    public void SaveGameData(GameData gameData)
    {
        try
        {
            var gameDataJson = JsonUtility.ToJson(gameData);
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

    public GameData LoadCurrentGameData()
    {
        var gameDataJson = PlayerPrefs.GetString(GAME_DATA);
        GameData gameData = JsonUtility.FromJson<GameData>(gameDataJson);
        Debug.Log(gameDataJson);
        return gameData;
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

    public void SaveDefaultGameData(GameData gameData)
    {
        var gameDataJson = JsonUtility.ToJson(gameData);
        PlayerPrefs.SetString(DEFAULT_GAME_DATA, gameDataJson);
        PlayerPrefs.Save();
    }
}
