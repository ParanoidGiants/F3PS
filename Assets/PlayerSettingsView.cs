using UnityEngine;
using F3PS;

public class PlayerSettingsView : MonoBehaviour
{
    void OnEnable()
    {
        GameManager.Instance.inputs.cursorInputForLook = false;
        GameManager.Instance.inputs.cursorLocked = false;
    }

    void OnDisable()
    {
        GameManager.Instance.inputs.cursorInputForLook = true;
        GameManager.Instance.inputs.cursorLocked = true;
    }
}
