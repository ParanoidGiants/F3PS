using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using DG.Tweening;

public class DonutShockwave : MonoBehaviour
{
    [Header("Debug")]
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
    public TimeObject timeObject;

    [Space(10)]
    [Header("Settings")]
    public float height = 1f;
    
    private Renderer ringRenderer;
    private Material ringMaterial;
    private Tween scaleTween;

    void Start()
    {
        ringRenderer = GetComponent<Renderer>();
        if (ringRenderer != null)
        {
            ringMaterial = ringRenderer.material;
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

        innerRadius = 0f;
        isActive = true;
        hitObjects.Clear();
        gameObject.SetActive(true);
        initialScale = transform.localScale;

        // Kill any existing tween
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
        }

        // Calculate animation duration based on expansion speed and max radius
        float duration = maxRadius / expansionSpeed;
        
        // Animate the scale using DOTween
        scaleTween = DOTween.To(() => 0f, (float value) => {
            innerRadius = value;
            outerRadius = value + shockWaveThickness;
            
            var scale = initialScale * outerRadius;
            scale.y = initialScale.y;
            transform.localScale = scale;

            // Update shader properties
            if (ringMaterial != null)
            {
                ringMaterial.SetFloat("_InnerRadius", innerRadius/outerRadius * 0.5f);
                ringMaterial.SetFloat("_OuterRadius", 0.5f);
            }
        }, maxRadius, duration)
        .SetEase(Ease.Linear)
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

        Hittable hittable = other.GetComponent<Hittable>();
        if (hittable == null) return;

        Debug.Log("Hit: " + hittable.name);

        if (hitObjects.Contains(hittable.owner.GetInstanceID()))
            return;

        if (Mathf.Abs(hittable.transform.position.y - center.y) > height)
            return;
        Debug.Log("Height: Check");

        float dist = Vector3.Distance(hittable.transform.position, center);
        if (dist < innerRadius && dist > outerRadius)
            return;
        Debug.Log("Distance: Check");

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