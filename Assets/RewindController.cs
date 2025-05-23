using System;
using Unity.VisualScripting;
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
        rewindHUD.UpdateRecordEffect(0);
        rewindHUD.gameObject.SetActive(false);
        _lineRenderer.enabled = false;

        if (hasCandidate && !selectedObjectForRecord)
        {
            currentCandidate.StopRecording();
            rewindHUD.UpdateRecordEffect(0);
            hasCandidate = false;
            currentCandidate = null;
        }
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
                    currentCandidate.SetupForPlayback();
                    rewindHUD.ShowPlaybackCircle(true);
                    isPlaybackActive = true;
                }
                else
                {
                    currentCandidate.SetupForRecording();
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
            contactPoint = currentCandidate.transform.position;
            return;
        }

        var physicsRecorder = GetCandidate();
        if (physicsRecorder == null)
        {
            return;
        }
        

        if (currentCandidate != physicsRecorder)
        {
            if (currentCandidate != null)
            {
                currentCandidate.Unpick();
            }
            physicsRecorder.SelectAsCandidate();
            currentCandidate = physicsRecorder;
            hasCandidate = true;
        }

        contactPoint = physicsRecorder.transform.position;
    }

    private PhysicsRecorder GetCandidate()
    {
        if (!_crosshair.CrosshairRaycast())
        {
            contactPoint = _crosshair.GetInfiniteDirection();
            if (hasCandidate)
            {
                hasCandidate = false;
                currentCandidate.Unpick();
                currentCandidate = null;
            }
            return null;
        }
        var physicsRecorder = _crosshair.Target.transform.GetComponent<PhysicsRecorder>();
        if (physicsRecorder == null)
        {
            contactPoint = _crosshair.GetInfiniteDirection();
            if (hasCandidate)
            {
                hasCandidate = false;
                currentCandidate.Unpick();
                currentCandidate = null;
            }
            return null;
        }
        return physicsRecorder;
    }
}
