using System;
using DG.Tweening;
using UnityEngine;

public enum DoorState
{
    OPENING,
    OPEN,
    CLOSING,
    CLOSED
}

public class DoorController : MonoBehaviour
{
    #region DEBUG
    [Header("Debug")]
    public bool debug = false;
    public bool openClose = false;

    #endregion DEBUG

    [Space(10)]
    [Header("Reference")]
    public Transform _door;
    public CameraRumble _cameraRumble;
    public TimeObject _timeObject;

    [Space(10)]
    [Header("Watcher")]
    public DoorState state = DoorState.CLOSED;
    public float _animationTime = 0f;
    public float _animationDuration = 5f;
    public float _openPosition = 1.5f;
    public float _closePosition = 0.5f;

    private void Update()
    {
        if (debug)
        {
            if ((state is DoorState.CLOSED || state is DoorState.CLOSING) && openClose)
            {
                OpenDoor();
            }

            if ((state is DoorState.OPEN || state is DoorState.OPENING) && !openClose)
            {
                CloseDoor();
            }
        }
        if (state is DoorState.OPENING)
        {
            _animationTime += _timeObject.ScaledDeltaTime;
            var position = _door.localPosition;
            position.y = Mathf.Lerp(_closePosition, _openPosition, _animationTime / _animationDuration);
            _door.localPosition = position;
            _cameraRumble.Rumble();
            if (_animationTime >= _animationDuration)
            {
                _animationTime = _animationDuration;
                state = DoorState.OPEN;
            }
        }
        else if (state is DoorState.CLOSING)
        {
            _animationTime -= _timeObject.ScaledDeltaTime;
            var position = _door.localPosition;
            position.y = Mathf.Lerp(_closePosition, _openPosition, _animationTime / _animationDuration);
            _door.localPosition = position;
            _cameraRumble.Rumble();
            if (_animationTime <= 0f)
            {
                _animationTime = 0f;
                state = DoorState.CLOSED;
            }
        }
    }
    public void OpenDoor()
    {
        Debug.Log("Open Door");
        if (state is DoorState.OPENING || state is DoorState.OPEN)
        {
            return;
        }
        state = DoorState.OPENING;
        Debug.Log("Opening Door");
    }

    public void CloseDoor()
    {
        Debug.Log("Close Door");
        if (state is DoorState.CLOSING || state is DoorState.CLOSED)
        {
            return;
        }
        state = DoorState.CLOSING;
        Debug.Log("Closing Door");
    }
}
