using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour
{
    public List<PhysicsRecorder> recorders = new List<PhysicsRecorder>();
    public float timeScale = 0.05f;
    void OnTriggerEnter(Collider other)
    {
        var o = other.GetComponent<PhysicsRecorder>();
        if (o == null || o.isInTimeBubble) return;

        o.StartRecording(transform.position, transform.localScale.x);
        recorders.Add(o);
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit");
        var o = other.GetComponent<PhysicsRecorder>();
        if (o == null)
        {
            return;
        }

        o.ChangeDirection();
    }

    private void FixedUpdate()
    {
        foreach (var  recorder in recorders)
        {
            recorder.OnFixedUpdate();
        }
    }

    private void OnDisable()
    {

        foreach (var recorder in recorders)
        {
            recorder.StopRecording();
        }
        recorders.Clear();
    }
}
