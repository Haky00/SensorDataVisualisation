using System;

namespace SensorDataVisualisation;

public readonly struct WaveParameters
{
    public WaveParameters(float amplitude, float phase, float frequency)
    {
        Amplitude = amplitude;
        Phase = phase;
        Frequency = frequency;
    }
    public readonly float Amplitude;
    public readonly float Phase;
    public readonly float Frequency;

    public float Compute(float time) {
        return Amplitude * MathF.Sin(Frequency * time + Phase);
    }
}
