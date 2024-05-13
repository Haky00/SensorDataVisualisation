using Godot;
using System;
using Num = System.Numerics;

namespace SensorDataVisualisation;

// This script controls legs movement and direction
public partial class LegScript : SkeletonPart
{
	// Material for the box that represents this bone
	[Export]
	public Material BoxMaterial = new();

	private CsgBox3D box;

	[Export]
	public Vector3 Velocity = Vector3.Zero;

	[Export]
	public float Direction = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		box = new()
		{
			Size = new(0.04f, Length, 0.04f),
			Name = "box",
			Material = BoxMaterial,
		};
		AddChild(box);
	}

	// Performs an update of the legs, velocity and direction must be set to change movement
	public void Update(double delta)
	{
		Loc += Velocity.Rotated(Vector3.Up, Direction) * (float)delta;
		Rot = Num.Quaternion.CreateFromAxisAngle(new(0, 1, 0), Direction);
		var transform = Transform;
		transform.Origin = Loc;
		Transform = transform;
	}

	private static float Sigmoid(float value)
	{
		float k = (float)Math.Exp(value);
		return (k / (1.0f + k) - 0.5f) * 2f;
	}

	public void UpdateBox()
	{
		Num.Vector3 rotLoc = Num.Vector3.Transform(new(0, Length / 2, 0), Rot);
		var transform = box.Transform;
		transform.Origin = new(rotLoc.X, rotLoc.Y, rotLoc.Z);
		transform.Basis = new(new Quaternion(Rot.X, Rot.Y, Rot.Z, Rot.W));
		box.Transform = transform;
	}
}
