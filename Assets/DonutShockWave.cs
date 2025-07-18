using UnityEngine;
using UnityEngine.ProBuilder;

public class DonutShockwave : MonoBehaviour
{
    public float expansionSpeed = 10f;
    public float thickness = 2f;
    public float maxRadius = 30f;
    public Vector3 center;
    private float innerRadius = 0f;
    private float outerRadius = 0f;
    private bool isActive = false;
    public LayerMask obstacleMask; // Set this in the inspector to define what blocks the shockwave
    public float height = 1f;
    public int damage = 10;

    public void StartShockwave(Vector3 position)
    {
        center = position;
        innerRadius = 0f;
        outerRadius = thickness;
        isActive = true;
    }

    void Update()
    {
        if (!isActive) return;

        float delta = expansionSpeed * Time.deltaTime;
        innerRadius += delta;
        outerRadius += delta;

        // Affect objects in the ring
        foreach (var obj in FindObjectsByType<Hittable>(FindObjectsSortMode.None))
        {
            if (obj.transform.position.y - center.y < -height)
                continue;
            float dist = Vector3.Distance(obj.transform.position, center);
            if (dist >= innerRadius && dist < outerRadius)
            {
                Vector3 direction = (obj.transform.position - center).normalized;
                float distance = dist;
                RaycastHit hit;
                // Raycast from center to object, using obstacleMask to check for blockers
                if (Physics.Raycast(center, direction, out hit, distance, obstacleMask | (1 << obj.gameObject.layer)))
                {
                    // If the first thing hit is the object itself, apply the effect
                    if (hit.transform == obj.transform)
                    {
                        obj.OnHit(damage, obj.transform.position - transform.position);
                    }
                    // Otherwise, something is blocking the wave
                }
                // else: no hit at all (shouldn't happen if object is there), so skip
            }
        }

        // Optionally, stop when max radius is reached
        if (outerRadius > maxRadius)
            isActive = false;
    }
}