using UnityEngine;

public static class MathUtils
{
    public static bool IsPositionInsideOfSphere(Vector3 position, Vector3 center, float radius)
    {
        return Vector3.Distance(position, center) <= radius;
    }


    public static Vector3? GetSphereIntersectionPoint(Vector3 center, float radius, Vector3 point, Vector3 direction)
    {
        // Normalize the direction to ensure accurate calculations.
        direction = direction.normalized;

        // Compute the vector from the sphere's center to the ray's origin.
        Vector3 m = point - center;

        // Coefficient for the linear term.
        float b = Vector3.Dot(m, direction);
        // The constant term.
        float c = Vector3.Dot(m, m) - radius * radius;

        // If point is outside the sphere and the ray is pointing away from the sphere, no intersection.
        if (c > 0f && b > 0f)
        {
            return null;
        }

        // Calculate the discriminant of the quadratic equation.
        float discriminant = b * b - c;
        if (discriminant < 0f)
        {
            // No real roots, so the line does not intersect the sphere.
            return null;
        }

        // Compute the smallest t value (the nearest intersection point along the ray).
        float t = -b - Mathf.Sqrt(discriminant);

        // If t is negative, it means the ray started inside the sphere,
        // so we take the other intersection (the exit point).
        if (t < 0f)
        {
            t = -b + Mathf.Sqrt(discriminant);
        }

        // Return the intersection point.
        return point + t * direction;
    }

    public static bool Vector3Equals(Vector3 a, Vector3 b)
    {
        // Using the overloaded operator for Vector3.
        return a == b;
    }

    public static bool QuaternionEquals(Quaternion a, Quaternion b)
    {
        // Using the overloaded operator for Quaternion.
        return a == b;
    }

    public static bool IntEquals(int a, int b)
    {
        return a == b;
    }
}