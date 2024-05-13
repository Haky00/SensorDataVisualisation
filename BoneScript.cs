using Godot;
using System;
using Num = System.Numerics;

namespace SensorDataVisualisation;

// This script controls bone movement and drawing, it is assigned to each bone
public partial class BoneScript : SkeletonPart
{
	// Path to the bone SkeletonPart to which this bone is attached to (can be null)
	[Export]
	public NodePath AttachedPath { get; set; }
	// Offset from the end of the parent bone, range between 0 (at the end) to 1 (at the start)
	[Export]
	public float AttachedOffset { get; set; } = 0;
	// Additional rotation from the parent (Y = roll)
	[Export]
	public Vector3 AttachedRotation { get; set; }
	// Material for the box that represents this bone
	[Export]
	public Material BoxMaterial = new();
	// Max angles for each of the 3 euler rotation directions (X and Z are horizontal rotations and Y is roll)
	[Export]
	public Vector3 MaxAngles { get; set; }

	[Export]
	public float Roll { get; set; }
	[Export]
	public float Angle { get; set; }
	[Export]
	public float Amount { get; set; }

	private CsgBox3D box;
	private SkeletonPart attached;

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
		if (AttachedPath is not null && !AttachedPath.IsEmpty)
		{
			attached = GetNode<SkeletonPart>(AttachedPath);
		}
	}

	// Performs an update of the bone, roll, angle and amount must be set to change the state
	public void Update()
	{
		Rot = Num.Quaternion.Identity;
		if (attached is not null)
		{
			Loc = attached.Loc;
			Rot = Num.Quaternion.Concatenate(Rot, attached.Rot);
			Num.Matrix4x4 rotMat = Num.Matrix4x4.CreateFromQuaternion(Rot);
			Num.Vector3 rotLoc = Num.Vector3.Transform(new(0, attached.Length * (1 - AttachedOffset), 0), rotMat);
			Loc += new Vector3(rotLoc.X, rotLoc.Y, rotLoc.Z);
			var transform = Transform;
			transform.Origin = Loc;
			Transform = transform;
		}
		Num.Quaternion attachedRot = Num.Quaternion.CreateFromYawPitchRoll(AttachedRotation.Y * (float)Math.PI / 180f, AttachedRotation.X * (float)Math.PI / 180f, AttachedRotation.Z * (float)Math.PI / 180f);

		float roll = Sigmoid(Roll) * (MaxAngles.Y * MathF.PI / 180.0f);
		float angleRadians = Angle;
		float xRotation = MathF.Cos(angleRadians) * Sigmoid(Amount) * MaxAngles.X * MathF.PI / 180.0f;
		float zRotation = MathF.Sin(angleRadians) * Sigmoid(Amount) * MaxAngles.Z * MathF.PI / 180.0f;
		Num.Quaternion yawPitchRotation = Num.Quaternion.CreateFromYawPitchRoll(0, xRotation, zRotation);
		Num.Quaternion rollRotation = Num.Quaternion.CreateFromYawPitchRoll(roll, 0, 0);
		Rot *= attachedRot * yawPitchRotation * rollRotation;
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
