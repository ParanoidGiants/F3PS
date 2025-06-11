using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public LayerMask whatIsShootable;
    public float sourceDistance;
    private RectTransform _rectTransform;
    private Camera _cam;

    private Ray ray;
    public Ray Ray => ray;
    private RaycastHit target;
    public RaycastHit Target => target;
    private bool isOnTarget;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _cam = Camera.main;
    }

    private void FixedUpdate()
    {
        ray = _cam.ScreenPointToRay(_rectTransform.position);
        var origin = ray.origin + ray.direction * sourceDistance;
        isOnTarget = Physics.Raycast(origin, ray.direction, out target, 100f, whatIsShootable);
    }

    public Vector3 GetTargetPosition()
    {
        var origin = ray.origin + ray.direction * sourceDistance;
        if (isOnTarget)
        {
            return target.point;
        }
        else
        {
            return origin + ray.direction * 100f;
        }
    }
    public bool CrosshairRaycast()
    {
        return isOnTarget;
    }

    public Vector3 GetInfiniteDirection()
    {
        return ray.origin + ray.direction * 100f;
    }
}
