using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitchesController : MonoBehaviour
{
    [Header("Debug")]
    public bool _areSwitchesFixed;

    [Space(10)]
    [Header("References")]
    public List<Switch> switches;
    public UnityEvent onSwitchTriggered;

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
        if (_areSwitchesFixed)
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
        onSwitchTriggered.Invoke();
        _areSwitchesFixed = true;
    }
}
