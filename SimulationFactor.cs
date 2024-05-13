using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SensorDataVisualisation;

// Represents a single factor in the simulation parameters
// A factor contains a constant value and a list of parameters for computing sines
public readonly struct SimulationFactor
{
    public SimulationFactor(float constant, IEnumerable<WaveParameters> sineParameters)
    {
        Constant = constant;
        SineParameters = sineParameters;
    }

    [JsonProperty("constant")]
    public readonly float Constant;
    
    [JsonProperty("sines")]
    public readonly IEnumerable<WaveParameters> SineParameters;

    public float Compute(float time) {
        return Constant + SineParameters?.Sum(parameters => parameters.Compute(time)) ?? 0;
    }
}
