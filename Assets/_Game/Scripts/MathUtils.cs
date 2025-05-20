using UnityEngine;

public static class MathUtils
{
    public static bool IsPositionInsideOfSphere(Vector3 position, Vector3 center, float radius)
    {
        return Vector3.Distance(position, center) <= radius;
    }

    public static Vector3? GetSphereIntersectionPoint(Vector3 center, float radius, Vector3 point, Vector3 direction)
    {
        direction = direction.normalized;
        Vector3 m = point - center;
        float b = Vector3.Dot(m, direction);
        float c = Vector3.Dot(m, m) - radius * radius;

        if (c > 0f && b > 0f)
        {
            return null;
        }

        float discriminant = b * b - c;
        if (discriminant < 0f)
        {
            return null;
        }

        float t = -b - Mathf.Sqrt(discriminant);
        if (t < 0f)
        {
            t = -b + Mathf.Sqrt(discriminant);
        }

        return point + t * direction;
    }

    public static bool Vector3Equals(Vector3 a, Vector3 b)
    {
        return a == b;
    }

    public static bool QuaternionEquals(Quaternion a, Quaternion b)
    {
        return a == b;
    }

    public static bool IntEquals(int a, int b)
    {
        return a == b;
    }
    public static Quaternion FindClosestPermutedRotation(Quaternion currentRotation, Quaternion referenceOrientation)
    {
        float[] angleSteps = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        Quaternion closestRotation = Quaternion.identity;
        float smallestAngle = float.MaxValue;
        Quaternion inverseReference = Quaternion.Inverse(referenceOrientation);
        Quaternion localRotation = inverseReference * currentRotation;

        foreach (float x in angleSteps)
        {
            foreach (float y in angleSteps)
            {
                foreach (float z in angleSteps)
                {
                    Quaternion candidateLocal = Quaternion.Euler(x, y, z);
                    Quaternion candidateWorld = referenceOrientation * candidateLocal;

                    float angle = Quaternion.Angle(candidateWorld, currentRotation);
                    if (angle < smallestAngle)
                    {
                        smallestAngle = angle;
                        closestRotation = candidateWorld;
                    }
                }
            }
        }

        return closestRotation;
    }

    public static Vector3 ClosestPointOnRay(Ray ray, Vector3 point)
    {
        Vector3 rayToPoint = point - ray.origin;
        float t = Vector3.Dot(rayToPoint, ray.direction);
        return ray.origin + t * ray.direction;
    }
}