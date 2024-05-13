using System;
using Newtonsoft.Json;

namespace SensorDataVisualisation;

// Parameters for a single sine
public readonly struct WaveParameters
{
    public WaveParameters(float amplitude, float phase, float frequency)
    {
        Amplitude = amplitude;
        Phase = phase;
        Frequency = frequency;
    }

    [JsonProperty("a")]
    public readonly float Amplitude;
    [JsonProperty("p")]
    public readonly float Phase;
    [JsonProperty("f")]
    public readonly float Frequency;

    public float Compute(float time) {
        return Amplitude * MathF.Sin(Frequency * time + Phase);
    }
}
