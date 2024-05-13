using Newtonsoft.Json;

namespace SensorDataVisualisation;

// Represents simulation parameters for a single bone
public readonly struct BoneParameters
{
    public BoneParameters(string boneName, SimulationFactor angle, SimulationFactor amount, SimulationFactor roll)
    {
        BoneName = boneName;
        Angle = angle;
        Amount = amount;
        Roll = roll;
    }

    [JsonProperty("name")]
    public readonly string BoneName;
    [JsonProperty("angle")]
    public readonly SimulationFactor Angle;
    [JsonProperty("amount")]
    public readonly SimulationFactor Amount;
    [JsonProperty("roll")]
    public readonly SimulationFactor Roll;
}
