using System;
using System.Linq;
using DG.Tweening;
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
    public MeshRenderer containerRenderer;
    public TimeObject timeObject;
    public bool isFilled = false;
    public UnityEvent isFilledEvent;

    private void OnEnable()
    {
        SwitchEventController.OnSwitchTriggered += OnSwitchTriggered;
        // pulsate container material's alpha to indicate interactability
        containerRenderer.material.DOKill();
        containerRenderer.material.DOFloat(0.5f, "_Alpha", 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnDisable()
    {
        SwitchEventController.OnSwitchTriggered -= OnSwitchTriggered;
    }


    public void Fill()
    {
        if (isFilled)
        {
            return;
        }

        fill += fillPerProjectile;
        fill = Mathf.Clamp01(fill);
        liquidRenderer.material.SetFloat("_Fill", fill);
        if (Mathf.Approximately(fill, 1f))
        {
            SwitchEventController.UpdateSwitchTriggered(gameObject.name);
            isFilledEvent.Invoke();
        }
    }

    private void OnSwitchTriggered(string id)
    {
        if (id != gameObject.name)
        {
            return;
        }
        isFilled = true;
        fill = 1;
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
