using System.Collections.Generic;
using UnityEngine;

public class SwitchesController : MonoBehaviour
{
    [Header("Debug")]
    public bool _isDoorOpen;

    [Space(10)]
    [Header("References")]
    public List<Switch> switches;
    public DoorController doorController;

    private void OnEnable()
    {
        foreach (var _switch in switches)
        {
            _switch.OnSwitchStateChanged += OnSwitchStateChanged;
        }
    }

    private void OnDsable()
    {
        foreach (var _switch in switches)
        {
            _switch.OnSwitchStateChanged -= OnSwitchStateChanged;
        }
    }

    private void OnSwitchStateChanged()
    {
        var areAllSwitchesTurnedOn = true;
        foreach (var _switch in switches)
        {
            areAllSwitchesTurnedOn &= _switch._isSwitchTurnedOn;
        }
        if (_isDoorOpen && !areAllSwitchesTurnedOn)
        {
            doorController.CloseDoor();
        }
        else if (!_isDoorOpen && areAllSwitchesTurnedOn)
        {
            doorController.OpenDoor();
        }

        _isDoorOpen = areAllSwitchesTurnedOn;
    }
}
