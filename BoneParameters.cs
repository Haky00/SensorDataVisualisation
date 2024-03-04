namespace SensorDataVisualisation;

public readonly struct BoneParameters
{
    public BoneParameters(string boneName, SimulationFactor angle, SimulationFactor amount, SimulationFactor roll)
    {
        BoneName = boneName;
        Angle = angle;
        Amount = amount;
        Roll = roll;
    }
    public readonly string BoneName;
    public readonly SimulationFactor Angle;
    public readonly SimulationFactor Amount;
    public readonly SimulationFactor Roll;
}
