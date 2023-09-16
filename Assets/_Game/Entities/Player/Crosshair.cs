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

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _cam = Camera.main;
    }

    public Vector3 GetTargetPosition()
    {

        var ray = _cam.ScreenPointToRay(_rectTransform.position);
        var origin = ray.origin + ray.direction * sourceDistance;
        RaycastHit hit;
        if (Physics.Raycast(origin, ray.direction, out hit, 100f, whatIsShootable))
        {
            return hit.point;
        }
        else
        {
            return origin + ray.direction * 100f;
        }
    }
}
