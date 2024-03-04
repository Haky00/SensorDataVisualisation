using System.Collections.Generic;
using System.Linq;

namespace SensorDataVisualisation;

public readonly struct SimulationFactor
{
    public SimulationFactor(float constant, IEnumerable<WaveParameters> sineParameters)
    {
        Constant = constant;
        SineParameters = sineParameters;
    }
    public readonly float Constant;
    public readonly IEnumerable<WaveParameters> SineParameters;

    public float Compute(float time) {
        return Constant + SineParameters.Sum(parameters => parameters.Compute(time));
    }
}
