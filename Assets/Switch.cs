using System;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [Header("Debug")]
    public int _entitiesOnSwitch;
    public bool _isSwitchTurnedOn;
    public bool _isSwitchFixedOn;
    public List<GameObject> _entities;
    public Renderer _renderer;

    [Header("References")]
    public Material switchOn;
    public Material switchOff;
    public Action OnSwitchStateChanged;

    internal void FixTurnOn()
    {
        _isSwitchFixedOn = true;
        _renderer.material = switchOn;
    }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isSwitchFixedOn)
        {
            return;
        }

        _entitiesOnSwitch++;

        if (!_isSwitchTurnedOn)
        {
            _renderer.material = switchOn;
            _isSwitchTurnedOn = true;
            OnSwitchStateChanged();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isSwitchFixedOn)
        {
            return;
        }

        _entitiesOnSwitch--;
        if (_entitiesOnSwitch == 0)
        {
            _isSwitchTurnedOn = false;
            _renderer.material = switchOff;
            OnSwitchStateChanged();
        }
    }
}
