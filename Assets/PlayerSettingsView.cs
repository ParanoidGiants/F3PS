using UnityEngine;
using F3PS;

public class PlayerSettingsView : MonoBehaviour
{
    void OnEnable()
    {
        GameManager.Instance.inputs.SetCursorLockedState(false);
    }

    void OnDisable()
    {
        GameManager.Instance.inputs.SetCursorLockedState(true);
    }
}
