using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MathsToolkit
{
    public static bool GetCirclesIntersection(Vector3 c1, float r1, Vector3 c2, float r2,
        out Vector3 a1, out Vector3 a2)
    {
        a1 = new Vector3();
        a2 = new Vector3();

        // Find the distance between the centers.
        float dx = c1.x - c2.x;
        float dy = c1.z - c2.z;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > r1 + r2)
        {
            return false;
        }
        else if (dist < Math.Abs(r1 - r2))
        {
            return false;
        }
        else if ((dist == 0) && (r1 == r2))
        {
            return false;
        }
        else
        {
            // Find a and h.
            double a = (r1 * r1 -
                r2 * r2 + dist * dist) / (2 * dist);
            double h = Math.Sqrt(r1 * r1 - a * a);

            // Find P2.
            double cx2 = c1.x + a * (c2.x - c1.x) / dist;
            double cy2 = c1.z + a * (c2.z - c1.z) / dist;

            // Get the points P3.
            a1 = new Vector3(
                (float)(cx2 + h * (c2.z - c1.z) / dist),
                0f,
                (float)(cy2 - h * (c2.x - c1.x) / dist));
            a2 = new Vector3(
                (float)(cx2 - h * (c2.z - c1.z) / dist),
                0f,
                (float)(cy2 + h * (c2.x - c1.x) / dist));

            // See if we have 1 or 2 solutions.
            return true;
        }
    }

    public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
    {
        float unsignedAngle = Vector3.Angle(from, to);

        float cross_x = from.y * to.z - from.z * to.y;
        float cross_y = from.z * to.x - from.x * to.z;
        float cross_z = from.x * to.y - from.y * to.x;
        float sign = Mathf.Sign(axis.x * cross_x + axis.y * cross_y + axis.z * cross_z);
        return unsignedAngle * sign;
    }
}
