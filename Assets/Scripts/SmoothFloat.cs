public class SmoothFloat
{
    private const byte SMOOTHING_STEPS = 5;

    private readonly float[] values = new float[SMOOTHING_STEPS];

    public void Add(float value)
    {
        for (int i = 0; i < values.Length - 1; i++)
        {
            values[i] = values[i + 1];
        }

        values[values.Length - 1] = value;
    }

    public float Get()
    {
        float sum = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum / values.Length;
    }

    public static implicit operator float(SmoothFloat smooth)
    {
        return smooth.Get();
    }
}
