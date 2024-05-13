using System.Collections.Generic;
using Newtonsoft.Json;

namespace SensorDataVisualisation;

// Contains all parameters for simulating a skeleton motion
public class SimulationParameters
{
    public SimulationParameters(string name, LegParameters legParameters, IEnumerable<BoneParameters> boneParameters)
    {
        Name = name;
        Legs = legParameters;
        Bones = boneParameters;
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("legs")]
    public LegParameters Legs { get; set; }
    
    [JsonProperty("bones")]
    public IEnumerable<BoneParameters> Bones { get; set; }
}
