using UnityEngine;

public class AggressiveSensor : MonoBehaviour
{
    public bool isTargetDetected;
    public Transform target;
    public int triggerCount;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
        
        isTargetDetected = true;
        triggerCount++;
        if (triggerCount > 1) return;
        target = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Helper.IsLayerPlayerLayer(other.gameObject.layer)) return;
        triggerCount--;
        if (triggerCount > 0) return;
        
        isTargetDetected = false;
        target = null;
    }
}
