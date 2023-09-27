using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    public float customTimeScale = 1.0f;

    private Rigidbody rb;
    private Vector3 lastVelocity;
    public bool isPitched = false;
    public bool pitch = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    double TOLERANCE = 0.001f;
    void FixedUpdate()
    {
        pitch = Math.Abs(customTimeScale - 1.0f) > TOLERANCE;
        
        
        if (pitch && !isPitched)
        {
            isPitched = true;
            lastVelocity = rb.velocity;
            rb.velocity = lastVelocity * customTimeScale;
            rb.useGravity = false;
        }

        if (isPitched)
        {
            
            if (fCollided)
            { // if collision happened...
                Debug.Log("ADJUST");
                fCollided = false; // reset flag
                // calculate acceleration due to collision
                var acc = (rb.velocity - fLastVel) * customTimeScale;
                // convert to force:
                // var force = rb.mass * acc;
                // call OnAfterCollision passing the Collision 
                // info and the reaction force:
                rb.velocity = Vector3.zero;
                rb.AddForce(acc, ForceMode.VelocityChange);
                rb.angularVelocity *= customTimeScale * 2f;
            }

            var newGravity = customTimeScale * Physics.gravity;
            rb.AddForce(newGravity, ForceMode.Acceleration);
            
            fLastVel = rb.velocity; // update last velocity
        }
    }
    
    private bool fCollided = false;
    private Vector3 fLastVel;
    private Collision fCollision;

    private void OnCollisionEnter(Collision coll){
        fCollision = coll; // save collision data
        fCollided = true; // signal that a collision happened
    }
}
