using DG.Tweening;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    #region DEBUG
    [Header("Debug")]
    public bool debug = false;
    public bool openClose = false;
    private void Update()
    {
        if (!debug)
        {
            return;
        }
        if (!_open && openClose)
        {
            OpenDoor();
        }

        if (_open && !openClose)
        {
            CloseDoor();
        }
    }

    #endregion DEBUG

    [Space(10)]
    [Header("Reference")]
    [SerializeField] private Transform _door;

    [Space(10)]
    [Header("Watcher")]
    [SerializeField] private bool _open;
    [SerializeField] private float _animationDuration = 5f;
    [SerializeField] private float _openPosition = 1.5f;
    [SerializeField] private float _closePosition = 0.5f;

    public void OpenDoor()
    {
        Debug.Log("Open Door");
        if (_open)
        {
            return;
        }
        Debug.Log("Open Door");
        _open = true;

        // Kill any existing animation on _door
        DOTween.Kill(_door);

        float animationDuration = _animationDuration * (_openPosition - _door.localPosition.y) / (_openPosition - _closePosition);
        _door.DOLocalMoveY(_openPosition, animationDuration);
    }

    private void CloseDoor()
    {
        if (!_open)
        {
            return;
        }
        _open = false;

        // Kill any existing animation on _door
        DOTween.Kill(_door);

        float animationDuration = _animationDuration * (_door.localPosition.y - _closePosition) / (_openPosition - _closePosition);
        _door.DOLocalMoveY(_closePosition, animationDuration);
    }
}
