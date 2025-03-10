using DG.Tweening;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Tweener _openAnimation;
    private Tweener _closeAnimation;

    [Header("Reference")]
    [SerializeField] private Transform _door;

    [Space(10)]
    [Header("Watcher")]
    [SerializeField] private bool _isOpen;
    [SerializeField] private float _animationDuration = 5f;
    [SerializeField] private float _openPosition = 1.5f;
    [SerializeField] private float _closePosition = 0.5f;

    [Space(10)]
    [Header("Debug")]
    public bool openClose = false;
    private void Update()
    {
        if (!_isOpen && openClose)
        {
            OnOpenDoor();
        }
        
        if (_isOpen && !openClose)
        {
            OnCloseDoor();
        }
    }

    private void OnOpenDoor()
    {
        if (_isOpen)
        {
            return;
        }
        _isOpen = true;
        DOTween.Kill(_closeAnimation);
        _openAnimation = _door.DOLocalMoveY(_openPosition, _animationDuration);
    }
    
    private void OnCloseDoor()
    {
        if (!_isOpen)
        {
            return;
        }
        _isOpen = false;
        DOTween.Kill(_openAnimation);
        _closeAnimation = _openAnimation = _door.DOLocalMoveY(_closePosition, _animationDuration);
    }
}
