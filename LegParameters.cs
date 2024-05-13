using Newtonsoft.Json;

namespace SensorDataVisualisation;

// Represents simulation parameters for legs
public class LegParameters
{
    public LegParameters(SimulationFactor velocityX, SimulationFactor velocityY, SimulationFactor velocityZ, SimulationFactor direction)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
        VelocityZ = velocityZ;
        Direction = direction;
    }

    [JsonProperty("velocityX")]
    public readonly SimulationFactor VelocityX;
    [JsonProperty("velocityY")]
    public readonly SimulationFactor VelocityY;
    [JsonProperty("velocityZ")]
    public readonly SimulationFactor VelocityZ;
    [JsonProperty("direction")]
    public readonly SimulationFactor Direction;
}
