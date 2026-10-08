using UnityEngine;

public class SmoothVector
{
    private const byte SMOOTHING_STEPS = 5;

    private readonly Vector3[] values = new Vector3[SMOOTHING_STEPS];

    public void Add(Vector3 value)
    {
        for (int i = 0; i < values.Length - 1; i++)
        {
            values[i] = values[i + 1];
        }

        values[values.Length - 1] = value;
    }

    public Vector3 Get()
    {
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum / values.Length;
    }

    public static implicit operator Vector3(SmoothVector s)
    {
        return s.Get();
    }
}