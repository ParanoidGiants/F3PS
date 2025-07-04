using F3PS;
using UnityEngine;

public class AnubisScrollController : MonoBehaviour
{
    private AnubisScrollSkillData AnubisScrollSkillData => GameManager.Instance.PlayerData.AnubisScrollSkillData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;

    [Header("References")]
    public SelectSkillControllerHUD selectSkillControllerHUD;
    public Animator animator;

    [Space(10)]
    [Header("Settings")]
    public LayerMask whatIsScrollable;

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
        PlayerEventController.SetAnubisScrollState(AnubisScrollState.None);
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
                animator.SetTrigger("AnubisScroll");
                currentCandidate.StartRecording();
                PlayerEventController.SetAnubisScrollCurrentFrame(1);
                PlayerEventController.SetAnubisScrollCurrentRecordingTime(0);
                PlayerEventController.SetAnubisScrollState(AnubisScrollState.Record);
                selectedObjectForRecord = true;
            }
            else
            {
                animator.SetTrigger("AnubisScroll");
                currentCandidate.StopRecording();
                PlayerEventController.SetAnubisScrollState(AnubisScrollState.None);
                currentCandidate.SelectAsCandidate();
                PlayerEventController.SetAnubisScrollCurrentFrame(0);
                PlayerEventController.SetAnubisScrollTotalFrames(0);
                selectedObjectForRecord = false;
                isPlaybackActive = false;
            }
        }

        if (selectedObjectForRecord && !wasActivatingPlaybackLastFrame && isActivatingPlaybackThisFrame)
        {
            if (!isPlaybackActive)
            {
                currentCandidate.SetupForPlayback();
                PlayerEventController.SetAnubisScrollState(AnubisScrollState.Paused);
                isPlaybackActive = true;
            }
            else
            {
                currentCandidate.SetupForRecording();
                PlayerEventController.SetAnubisScrollState(AnubisScrollState.Record);
                isPlaybackActive = false;
            }
        }

        if (isPlaybackActive)
        {

            currentCandidate.Playback(AnubisScrollSkillData.ScrollSpeed * forwardBackward);
        }
    }

    public void OnFixedUpdateForCurrentCandidate()
    {
        if (selectedObjectForRecord && AnubisScrollSkillData.State == AnubisScrollState.Record)
        {
            currentCandidate.Record();
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
