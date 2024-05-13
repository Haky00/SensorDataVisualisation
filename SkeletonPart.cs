using Godot;
using Num = System.Numerics;

namespace SensorDataVisualisation;

// Base class for legs and bones
public partial class SkeletonPart : Node3D
{
	[Export]
	public float Length { get; set; }

	public Vector3 Loc;
	public Num.Quaternion Rot;

}
