using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

public class BezierSpline : MonoBehaviour
{
    public enum PointMode
    {
        Free,
        Aligned,
        Mirrored
    }

    [SerializeField]
    private List<Vector3> points;

    [SerializeField]
    private List<PointMode> modes;

    public bool debugShowDirection = false;

    public int ControlPointCount
    {
        get
        {
            return points.Count;
        }
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (index > 0)
            {
                points[index - 1] += delta;
            }
            if (index + 1 < points.Count)
            {
                points[index + 1] += delta;
            }
        }
        points[index] = point;
        EnforceMode(index);
    }

    public PointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, PointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        PointMode mode = modes[modeIndex];
        if (mode == PointMode.Free || (modeIndex == 0 || modeIndex == modes.Count - 1))
        {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = points.Count - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Count)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Count)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Count - 2;
            }
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == PointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public int CurveCount
    {
        get
        {
            return (points.Count - 1) / 3;
        }
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Count - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetForward(float t)
    {
        return GetVelocity(t).normalized;
    }

    public Vector3 GetRight(float t)
    {
        return Vector3.Cross(Vector3.up, GetForward(t)).normalized;
    }

    public Vector3 GetUp(float t)
    {
        Vector3 forward = GetForward(t);
        return Vector3.Cross(forward, Vector3.Cross(Vector3.up, forward)).normalized;
    }

    public void AddCurve()
    {
        Vector3 forward = GetForward(1.0f);
        Vector3 point = points[points.Count - 1];
        point += forward * 5;
        points.Add(point);
        point += forward * 5;
        points.Add(point);
        point += forward * 5;
        points.Add(point);

        modes.Add(modes[modes.Count - 1]);
        EnforceMode(points.Count - 4);
    }

    public void RemoveCurve(int index)
    {
        int pointIndex = index * 3 - 1;
        if (pointIndex < 0) pointIndex = 0;
        if (pointIndex + 3 > points.Count - 1) pointIndex--;
        points.RemoveRange(pointIndex, 3);

        modes.RemoveAt(index);
    }

    public void Reset()
    {
        points = new List<Vector3> {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
        modes = new List<PointMode> {
            PointMode.Free,
            PointMode.Free
        };
    }
}