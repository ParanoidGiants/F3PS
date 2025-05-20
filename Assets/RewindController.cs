using System;
using UnityEngine;

public class RewindController : MonoBehaviour
{
    [Header("UI References")]
    public RewindHUD rewindHUD;

    [Space(10)]
    [Header("Settings")]
    public LayerMask rewindableLayer;
    public float rewindSpeed = 5.0f;
    public float minimumDistance = 3f;
    public float maximumDistance = 50f;

    [Space(10)]
    [Header("Watchers")]
    public PhysicsRecorder currentCandidate;
    public Vector3 contactPoint;
    public bool selectedObjectForRecord = false;
    public bool hasCandidate = false;
    public bool wasRecordingLastFrame = false;
    public bool wasActivatingPlaybackLastFrame = false;
    public bool isPlaybackActive = false;

    private LineRenderer _lineRenderer;
    private Crosshair _crosshair;

    private void Awake()
    {
        _crosshair = FindObjectOfType<Crosshair>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        rewindHUD.gameObject.SetActive(true);
        _lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        rewindHUD.gameObject.SetActive(false);
        _lineRenderer.enabled = false;
    }

    public void OnUpdate(bool isRecording, bool activatePlayback, float forwardBackward)
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, contactPoint);

        if (hasCandidate)
        {
            if (!wasRecordingLastFrame && isRecording)
            {
                if (!selectedObjectForRecord)
                {
                    currentCandidate.StartRecording();
                    rewindHUD.UpdateRecordEffect(0);
                    selectedObjectForRecord = true;
                }
                else
                {
                    currentCandidate.StopRecording();
                    rewindHUD.UpdateRecordEffect(0);
                    selectedObjectForRecord = false;
                    isPlaybackActive = false;
                }
            }

            if (selectedObjectForRecord && !wasActivatingPlaybackLastFrame && activatePlayback)
            {
                if (!isPlaybackActive)
                {
                    currentCandidate.ChangeToPlayback();
                    rewindHUD.ShowPlaybackCircle(true);
                    isPlaybackActive = true;
                }
                else
                {
                    currentCandidate.ChangeToRecord();
                    rewindHUD.ShowPlaybackCircle(false);
                    isPlaybackActive = false;
                }
            }

            if (isPlaybackActive)
            {
                currentCandidate.Playback(rewindSpeed * forwardBackward);
                rewindHUD.UpdatePlaybackEffect(currentCandidate.GetPlaybackPercentage());
            }
            else
            {
                rewindHUD.UpdateRecordEffect(currentCandidate.GetPlaybackPercentage());
            }
        }
        wasRecordingLastFrame = isRecording;
        wasActivatingPlaybackLastFrame = activatePlayback;

    }

    public void OnFixedUpdate()
    {
        if (selectedObjectForRecord)
        {
            // currentCandidate.OnFixedUpdate();
            return;
        }
        if (!_crosshair.CrosshairRaycast())
        {
            contactPoint = _crosshair.GetInfiniteDirection();

            if (hasCandidate)
            {
                hasCandidate = false;
                currentCandidate.Unpick();
                currentCandidate = null;
            }
            return;
        }
        var target = _crosshair.Target;
        contactPoint = target.point;
        var movable = target.transform.GetComponent<PhysicsRecorder>();
        if (movable == currentCandidate)
        {
            return;
        }

        if (hasCandidate)
        {
            hasCandidate = false;
            currentCandidate.Unpick();
            currentCandidate = null;
        }
        if (movable != null)
        {
            hasCandidate = true;
            movable.SelectAsCandidate();
            currentCandidate = movable;
        }
    }
}
