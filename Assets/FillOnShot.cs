using System;
using System.Linq;
using F3PS;
using UnityEngine;
using UnityEngine.Events;

public class FillOnShot : MonoBehaviour
{
    private SwitchesData SwitchesData => GameManager.Instance.GameData.SwitchesData;
    private SwitchEventController SwitchEventController => GameManager.Instance.saveGameManager.SwitchEventController;
    public float fill = 0f;
    public float fillPerProjectile = 0.2f;
    public float unfillPerSecond = 0.2f;
    public MeshRenderer liquidRenderer;
    public TimeObject timeObject;
    public bool isFilled = false;
    public UnityEvent isFilledEvent;

    private void Start()
    {
        var switchData = SwitchesData.Switches.First(s => s.Id == gameObject.name);
        if (switchData == null)
        {
            Debug.LogWarning("Switch not registered");
            return;
        }
        if (switchData.IsTriggered)
        {
            isFilled = true;
            fill = 1;
            isFilledEvent.Invoke();
            return;
        }
    }

    private void OnEnable()
    {
        SwitchEventController.OnSwitchTriggered += OnSwitchTriggered;
    }

    private void OnDisable()
    {
        SwitchEventController.OnSwitchTriggered -= OnSwitchTriggered;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (isFilled)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<OsirisKickProjectile>(out var _) || collision.gameObject.TryGetComponent<HorusPalmProjectile>(out var _))
        {
            fill += fillPerProjectile;
            fill = Mathf.Clamp01(fill);
            liquidRenderer.material.SetFloat("_Fill", fill);
            if (fill == 1f)
            {
                SwitchEventController.UpdateSwitchTriggered(gameObject.name);
            }
        }
    }

    private void OnSwitchTriggered(string id)
    {
        if (id != gameObject.name)
        {
            return;
        }
        isFilled = true;
        isFilledEvent.Invoke();
    }

    private void Update()
    {
        if (isFilled)
        {
            return;
        }

        fill -= timeObject.ScaledDeltaTime * unfillPerSecond;
        fill = Mathf.Clamp01(fill);
        liquidRenderer.material.SetFloat("_Fill", fill);
    }
}
