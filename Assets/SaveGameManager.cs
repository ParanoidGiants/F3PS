using UnityEngine;
using F3PS;

public class SaveGameManager : MonoBehaviour
{

    public string currentPlayerDataKey = "PlayerData00";
    public void SaveCurrentPlayerData()
    {
        PlayerData playerData = GameManager.Instance.PlayerData;
        var playerDataJson = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(currentPlayerDataKey, playerDataJson);
        PlayerPrefs.Save();
    }

    public void LoadCurrentPlayerData()
    {
        var playerDataJson = PlayerPrefs.GetString(currentPlayerDataKey);
        if (string.IsNullOrEmpty(playerDataJson))
        {
            return;
        }
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(playerDataJson);
        GameManager.Instance.PlayerData = playerData;
    }
}
