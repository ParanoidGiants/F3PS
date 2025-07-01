using F3PS;
using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour
{
    private TimeBubbleSkillData TimeBubbleData => GameManager.Instance.PlayerData.TimeBubbleSkillData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;

    public List<TimeObject> timeObjects = new List<TimeObject>();
    private Renderer _renderer;
    public Color baseColor = Color.white;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        PlayerEventController.OnTimeBubbleTimeScaleChanged += UpdateTimeScale;
    }

    private void OnEnable()
    {
        _renderer.material.SetFloat("_BrackeysMoveSpeed", TimeBubbleData.TimeScale * 0.25f);
    }

    void OnTriggerEnter(Collider other)
    {
        var timeObject = other.GetComponent<TimeObject>();
        if (timeObject == null)
        {
            return;
        }
        timeObjects.Add(timeObject);
        timeObject.Activate(TimeBubbleData.TimeScale);
    }
    
    void OnTriggerExit(Collider other)
    {
        var timeObject = other.GetComponent<TimeObject>();
        if (timeObject == null)
        {
            return;
        }
        timeObjects.Remove(timeObject);
        timeObject.Deactivate();
    }

    public void Clear()
    {
        foreach (var timeObject in timeObjects)
        {
            timeObject.Deactivate();
        }
        timeObjects.Clear();
    }

    private void UpdateTimeScale(float timeScale)
    {
        var emissionColor = Color.Lerp(Color.white, baseColor, timeScale);
        _renderer.material.SetFloat("_BrackeysMoveSpeed", timeScale * 0.25f);
        _renderer.material.SetColor("_BrackeysEmission", emissionColor);
        foreach (var timeObject in timeObjects)
        {
            timeObject.PitchTimeScale(timeScale);
        }
    }
}
