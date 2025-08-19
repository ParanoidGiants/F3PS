using UnityEngine;

public class DeactivateOnStart : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
