using UnityEngine;
using UnityEngine.Events;

public class TriggerEventOnDestroy : MonoBehaviour
{
    public UnityEvent triggerEvent;

    private void OnDestroy()
    {
        triggerEvent.Invoke();
    }
}
