using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyInterps
{
    public static Vector3 Sinerp(Vector3 start, Vector3 end, float t)
    {
        t = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI * 0.5f);

        return new Vector3(
            start.x + (end.x - start.x) * t,
            start.y + (end.y - start.y) * t,
            start.z + (end.z - start.z) * t
            );
    }

    public static Vector3 Coserp(Vector3 start, Vector3 end, float t)
    {
        t = 1- Mathf.Cos(Mathf.Clamp01(t) * Mathf.PI * 0.5f);

        return new Vector3(
            start.x + (end.x - start.x) * t,
            start.y + (end.y - start.y) * t,
            start.z + (end.z - start.z) * t
            );
    }

    // Eases in with a quadratic function for ease_in/2 seconds and the interpolates linearly. 
    // No ease out. 
    // Speed is the slope of the linear interpolation. 
    // The curve is continuous, i.e. the deriative of the functions at the junctions is the same. 
    public static Vector3 QuadEaseIn(Vector3 start, Vector3 target, out bool done, float t, float ease_in, float speed)
    {
        Vector3 direction = (target - start);
        float   t_, mag   = direction.magnitude; // Move this calculation to when the target is set!
        direction = direction.normalized;

        if (t <= ease_in * 0.5f && ease_in > 0)
            t_ = speed * t * t / ease_in;
        else
            t_ = speed * (t - ease_in * 0.25f);

        t_   = Mathf.Min(t_, mag);
        done = t_ == mag;

        return new Vector3(
            start.x + direction.x * t_,
            start.y + direction.y * t_,
            start.z + direction.z * t_
            );
    }
}

static class ExtensionMethods
{
    public static Vector3 Round(this Vector3 vec, Vector3 axis)
    {
        return new Vector3(
            axis.x != 0 ? Mathf.Round(vec.x) : vec.x,
            axis.y != 0 ? Mathf.Round(vec.y) : vec.y,
            axis.z != 0 ? Mathf.Round(vec.z) : vec.z);
    }

    public static Vector3 Floor(this Vector3 vec, Vector3 axis)
    {
        return new Vector3(
            axis.x != 0 ? Mathf.Floor(vec.x) : vec.x,
            axis.y != 0 ? Mathf.Floor(vec.y) : vec.y,
            axis.z != 0 ? Mathf.Floor(vec.z) : vec.z);
    }

    public static float L1Norm(this Vector3 vec)
    {
        
        return vec.x + vec.y + vec.z;
    }
}
