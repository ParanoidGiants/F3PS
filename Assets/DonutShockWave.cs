using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DonutShockwave : MonoBehaviour
{
    [Header("Debug")]
    public Material ringMaterial;
    public Tween scaleTween;
    public Color originalColor;
    public float fadeProgress;
    public bool isActive = false;
    public Vector3 initialScale;
    public Vector3 center;
    public float expansionSpeed = 10f;
    public float thickness = 2f;
    public float maxRadius = 30f;
    public float innerRadius = 0f;
    public float outerRadius = 0f;
    public int damage = 10;
    public float timeScale = 1f;
    public List<int> hitObjects = new List<int>();

    [Space(10)]
    [Header("References")]
    public Collider ringCollider;
    public Renderer ringRenderer;
    public CameraShake cameraShake;

    [Space(10)]
    [Header("Settings")]
    public float height = 1f;
    public float fadeOutDuration = 1f; // Duration of fade out effect in seconds
    

    void Awake()
    {
        ringMaterial = ringRenderer.material;
        originalColor = ringMaterial.color;
        gameObject.SetActive(false);
    }

    public void Init(Collider[] collidersToIgnore)
    {
        foreach (var colliderToIgnore in collidersToIgnore)
        {
            Physics.IgnoreCollision(ringCollider, colliderToIgnore);
        }
    }

    public void StartShockwave(
        Vector3 position,
        int shockWaveDamage,
        float shockWaveExpansionSpeed,
        float shockWaveThickness,
        float shockWaveMaxRadius,
        float shockWavetimeScale
    ) {
        center = position;
        outerRadius = shockWaveThickness;
        expansionSpeed = shockWaveExpansionSpeed;
        maxRadius = shockWaveMaxRadius;
        damage = shockWaveDamage;
        timeScale = shockWavetimeScale;

        Debug.Log(timeScale);

        gameObject.SetActive(true);
        innerRadius = 0f;
        fadeProgress = 0f;
        isActive = true;
        hitObjects.Clear();
        initialScale = transform.localScale;

        Color color = originalColor;
        color.a = 1f;
        ringMaterial.color = color;
        
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
        }

        cameraShake.Shake(1f);

        float duration = maxRadius / (expansionSpeed * timeScale);
        scaleTween = DOTween.To(() => 0f, (float value) => {
            innerRadius = value;
            outerRadius = value + shockWaveThickness;
            
            var scale = Vector3.one * outerRadius;
            scale.z = 1f;
            transform.localScale = scale;

            // Calculate fade out progress
            if (value >= maxRadius - fadeOutDuration * expansionSpeed)
            {
                float fadeStartValue = maxRadius - fadeOutDuration * expansionSpeed;
                fadeProgress = (value - fadeStartValue) / (fadeOutDuration * expansionSpeed);
                fadeProgress = Mathf.Clamp01(fadeProgress);
            }

            // Update shader properties
            if (ringMaterial != null)
            {
                ringMaterial.SetFloat("_InnerRadius", 0);
                ringMaterial.SetFloat("_OuterRadius", 0.49f);
                
                // Update alpha for fade out
                Color color = originalColor;
                color.a = 1f - fadeProgress;
                ringMaterial.color = color;
            }
        }, maxRadius, duration)
        .SetEase(Ease.OutCubic)
        .OnComplete(() => {
            isActive = false;
            transform.localScale = initialScale;
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        if (!isActive) return;

        transform.position = center;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (fadeProgress > 0f)
        {
            return;
        }

        Hittable hittable = other.GetComponent<Hittable>();
        if (hittable == null) return;


        if (hitObjects.Contains(hittable.owner.GetInstanceID()))
            return;

        if (Mathf.Abs(hittable.transform.position.y - center.y) > height)
            return;

        float dist = Vector3.Distance(hittable.transform.position, center);
        if (dist < innerRadius && dist > outerRadius)
            return;

        hittable.OnHit(damage, hittable.transform.position - transform.position);
        hitObjects.Add(hittable.owner.GetInstanceID());
    }

    void OnDestroy()
    {
        // Clean up tween when object is destroyed
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
        }
    }
}