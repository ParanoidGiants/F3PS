using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    private RectTransform _rectTransform;
    private RectTransform _canvasRectTransform;
    private CinemachineBrain _cinemachineBrain;
    
    public GameObject bar;
    public Transform target;
    public Image fillImage;
    public Vector2 offset;
    public bool isTargetSet = false;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasRectTransform = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
    }
    
    private void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(UpdateUI);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdateUI);
    }

    private void UpdateUI(CinemachineBrain brain)
    {
        if (brain != _cinemachineBrain || !isTargetSet) 
        {
            return;
        }

        var enemyDirection = target.position - _cinemachineBrain.transform.position;
        if (Vector3.Dot(_cinemachineBrain.transform.forward, enemyDirection) <= 0)
        {
            bar.SetActive(false);
            return;
        }
        bar.SetActive(true);
        _rectTransform.anchoredPosition = GetCanvasAnchoredPosition(target.position) + offset;
    }

    public Vector2 GetCanvasAnchoredPosition(Vector3 worldPosition)
    {
        Vector2 viewportPosition= _cinemachineBrain.OutputCamera.WorldToViewportPoint(target.position);
        var sizeDelta = _canvasRectTransform.sizeDelta;
        return new Vector2(
            viewportPosition.x * sizeDelta.x - sizeDelta.x * 0.5f,
            viewportPosition.y * sizeDelta.y - sizeDelta.y * 0.5f
        );
    }

    public void SetTarget(Transform target)
    {
        isTargetSet = target != null;
        this.target = target;
    }

    public void SetFill(float factor)
    {
        fillImage.fillAmount = factor;
    }
}
