using UnityEngine;

public class AggressiveSensor : MonoBehaviour
{
    public bool isTargetDetected;
    public Hittable target;
    public int triggerCount;
    
    private void OnTriggerEnter(Collider other)
    {
        var hittable = other.GetComponent<Hittable>();
        if (!hittable || !Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;

        target = hittable;
        isTargetDetected = true;
        triggerCount++;
        if (triggerCount > 1) return;
    }

    private void OnTriggerExit(Collider other)
    {
        var hittable = other.GetComponent<Hittable>();
        if (!hittable || !Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
        
        triggerCount--;
        
        if (triggerCount > 0) return;
        target = null;
        isTargetDetected = false;
    }
}
