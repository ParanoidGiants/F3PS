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

    private void OnDisable()
    {
        foreach (var _switch in switches)
        {
            _switch.OnSwitchStateChanged -= OnSwitchStateChanged;
        }
    }

    private void OnSwitchStateChanged()
    {
        if (_isDoorOpen)
        {
            return;
        }
        var areAllSwitchesTurnedOn = true;
        foreach (var _switch in switches)
        {
            areAllSwitchesTurnedOn &= _switch._isSwitchTurnedOn;
        }
        if (!areAllSwitchesTurnedOn)
        {
            return;
        }

        foreach (var _switch in switches)
        {
            _switch.FixTurnOn();
        }
        doorController.OpenDoor();
        _isDoorOpen = true;
    }
}
