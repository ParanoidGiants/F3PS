using UnityEngine;

public class LoadNextPuzzle : MonoBehaviour
{
    public string nextPuzzleSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneLoader.Instance.LoadScene(nextPuzzleSceneName);
        }
    }
}
