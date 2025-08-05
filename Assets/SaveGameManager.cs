using UnityEngine;
using F3PS;
using System;

public class SaveGameManager : MonoBehaviour
{
    private const string GAME_SAVE_DATA = "GameSaveData";

    public void SavePlayerData(int spawnPointIndex)
    {
        PlayerData playerData = GameManager.Instance.PlayerData;
        var playerDataJson = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(GAME_SAVE_DATA, playerDataJson);
        PlayerPrefs.Save();
    }

    public void LoadCurrentPlayerData()
    {
        var playerDataJson = PlayerPrefs.GetString(GAME_SAVE_DATA);
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(playerDataJson);
        GameManager.Instance.PlayerData = playerData;
    }

    public bool HasGameSaveData()
    {
        return PlayerPrefs.HasKey(GAME_SAVE_DATA);
    }
}
