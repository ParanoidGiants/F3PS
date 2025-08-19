using F3PS;
using UnityEngine;

public abstract class SetModelValueView : MonoBehaviour
{
    protected PlayerData PlayerData => GameManager.Instance.GameData.PlayerData;
    protected PlayerEventController PlayerEventController => GameManager.Instance.GameData.PlayerEventController;
}
