using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    private RectTransform _rectTransform;
    private CinemachineBrain _cinemachineBrain;
    
    public GameObject bar;
    public Transform target;
    public Image fillImage;
    public Vector2 offset;
    public bool isTargetSet = false;
    public float scaleAtMinDistance = 1.0f;
    public float scaleAtMaxDistance = 0.5f;
    public float minDistance = 5f;
    public float maxDistance = 30f;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
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
        float distance = Vector3.Distance(_cinemachineBrain.transform.position, target.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float scale = Mathf.Lerp(scaleAtMinDistance, scaleAtMaxDistance, t);
        _rectTransform.localScale = Vector3.one * scale;

        Vector2 dynamicOffset = offset * scale * scale;
        _rectTransform.anchoredPosition = GetCanvasAnchoredPosition(target.position) + dynamicOffset;
    }

    public Vector2 GetCanvasAnchoredPosition(Vector3 worldPosition)
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            screenPoint,
            null,
            out localPoint
        );
        return localPoint;
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
