using Godot;
using System;
using Num = System.Numerics;

namespace SensorDataVisualisation;

public partial class BoneScript : Node3D
{
	[Export]
	public NodePath AttachedPath { get; set; }
	[Export]
	public float AttachedOffset { get; set; } = 0;
	[Export]
	public Vector3 AttachedRotation { get; set; }
	[Export]
	public float Length { get; set; }

	[Export]
	public Material BoxMaterial = new();

	[Export]
	public Vector3 MaxAngles { get; set; }

	[Export]
	public float Roll { get; set; }
	[Export]
	public float Angle { get; set; }
	[Export]
	public float Amount { get; set; }

	[Export]
	public float RollFactor { get; set; } = 1f;
	[Export]
	public float AngleFactor { get; set; } = 1f;
	[Export]
	public float AmountFactor { get; set; } = 1f;

	public Vector3 Loc;
	public Num.Quaternion Rot;
	private CsgBox3D box;
	private BoneScript attached;

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
			attached = GetNode<BoneScript>(AttachedPath);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private double time = 0;

	public void Animate(double delta)
	{
		time += delta;
		Roll = (float)Math.Sin(time * RollFactor);
		Amount = (float)Math.Sin(time * AmountFactor);
		Angle = (float)Math.Sin(time * AngleFactor) * 360;
	}

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

		float roll = Sigmoid(Roll) * (MaxAngles.Y * (float)Math.PI / 180.0f);
		float angleRadians = Angle * (float)Math.PI / 180.0f;
		float xRot = (float)Math.Cos(angleRadians) * Sigmoid(Amount) * MaxAngles.X * (float)Math.PI / 180.0f;
		float zRot = (float)Math.Sin(angleRadians) * Sigmoid(Amount) * MaxAngles.Z * (float)Math.PI / 180.0f;
		Num.Quaternion angleRot = Num.Quaternion.CreateFromYawPitchRoll(0, xRot, zRot);
		Num.Quaternion rollRot = Num.Quaternion.CreateFromYawPitchRoll(roll, 0, 0);

		Rot = Num.Quaternion.Concatenate(Num.Quaternion.Concatenate(Num.Quaternion.Concatenate(rollRot, angleRot), attachedRot), Rot);
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
