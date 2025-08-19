using System.Collections;
using F3PS;
using TMPro;
using UnityEngine;

public class SpawnPointDebugText : MonoBehaviour
{
    public TextMeshProUGUI spawnPointText;

    private void OnEnable()
    {
        GameManager.Instance.GameData.PlayerEventController.OnCurrentSpawnPointChanged += OnCurrentSpawnPointChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GameData.PlayerEventController.OnCurrentSpawnPointChanged -= OnCurrentSpawnPointChanged;
    }


    private void OnCurrentSpawnPointChanged(int spawnPoint)
    {
        StartCoroutine(ShowText(spawnPoint));
    }

    private IEnumerator ShowText(int spawnPoint)
    {
        spawnPointText.text = $"Spawn Point {spawnPoint} reached";
        spawnPointText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        spawnPointText.gameObject.SetActive(false);
    }
}
