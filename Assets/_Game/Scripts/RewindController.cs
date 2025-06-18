using System;
using UnityEngine;

public class RewindController : MonoBehaviour
{
    [Header("References")]
    public RewindHUD rewindHUD;
    public SelectSkillControllerHUD selectSkillControllerHUD;
    public Animator animator;

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
    public bool isRecordingThisFrame = false;
    public bool wasRecordingLastFrame = false;
    public bool isActivatingPlaybackThisFrame = false;
    public bool wasActivatingPlaybackLastFrame = false;
    public bool isPlaybackActive = false;

    private LineRenderer _lineRenderer;
    private Crosshair _crosshair;

    private void Awake()
    {
        _crosshair = FindFirstObjectByType<Crosshair>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        _lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        _lineRenderer.enabled = false;

        if (hasCandidate && !selectedObjectForRecord)
        {
            currentCandidate.StopRecording();
            hasCandidate = false;
            currentCandidate = null;
        }
    }

    public void OnUpdate(bool isRecording, bool activatePlayback, float forwardBackward)
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, contactPoint);
        wasRecordingLastFrame = isRecordingThisFrame;
        isRecordingThisFrame = isRecording;
        wasActivatingPlaybackLastFrame = isActivatingPlaybackThisFrame;
        isActivatingPlaybackThisFrame = activatePlayback;

        if (!hasCandidate)
        {
            return;
        }

        if (!wasRecordingLastFrame && isRecordingThisFrame)
        {
            if (!selectedObjectForRecord)
            {
                animator.SetTrigger("Rewind");
                rewindHUD.SetRecording();
                currentCandidate.StartRecording();
                rewindHUD.UpdateRecordEffect(0);
                selectedObjectForRecord = true;
            }
            else
            {
                animator.SetTrigger("Rewind");
                rewindHUD.SetNone();
                currentCandidate.StopRecording();
                currentCandidate.SelectAsCandidate();
                rewindHUD.UpdateRecordEffect(0);
                selectedObjectForRecord = false;
                isPlaybackActive = false;
            }
        }

        if (selectedObjectForRecord && !wasActivatingPlaybackLastFrame && isActivatingPlaybackThisFrame)
        {
            if (!isPlaybackActive)
            {
                rewindHUD.SetPausing();
                currentCandidate.SetupForPlayback();
                rewindHUD.ShowPlaybackBar(true);
                isPlaybackActive = true;
            }
            else
            {
                rewindHUD.SetRecording();
                currentCandidate.SetupForRecording();
                rewindHUD.ShowPlaybackBar(false);
                isPlaybackActive = false;
            }
        }

        if (isPlaybackActive)
        {
            if (forwardBackward > 0)
            {
                rewindHUD.SetPlaying();
            }
            else if (forwardBackward < 0)
            {
                rewindHUD.SetRewinding();
            }
            else
            {
                rewindHUD.SetPausing();
            }
            currentCandidate.Playback(rewindSpeed * forwardBackward);
        }

        var playbackPercentage = currentCandidate.GetPlaybackPercentage();
        if (playbackPercentage == 0f && !isPlaybackActive)
        {
            rewindHUD.UpdatePlaybackEffect(1f);
        }
        else
        {
            rewindHUD.UpdatePlaybackEffect(playbackPercentage);
        }
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

    public bool IsAiming()
    {
        return !wasRecordingLastFrame && isRecordingThisFrame;
    }

    internal void OnLateUpdate()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, contactPoint);
    }
}
