using Godot;
using System;
using Num = System.Numerics;

namespace SensorDataVisualisation;

public partial class SkeletonPhoneScript : Node3D
{
	[Export]
	public NodePath AttachedPath { get; set; }
	[Export]
	public float AttachedOffset { get; set; } = 0;
	[Export]
	public Vector3 AttachedRotation { get; set; }

	private BoneScript attached;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (AttachedPath is not null && !AttachedPath.IsEmpty)
		{
			attached = GetNode<BoneScript>(AttachedPath);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Update()
	{
		if (attached is null)
		{
			return;
		}
		Vector3 Loc = attached.Loc;
		Num.Quaternion Rot = attached.Rot;
		Num.Matrix4x4 rotMat = Num.Matrix4x4.CreateFromQuaternion(Rot);
		Num.Vector3 rotLoc = Num.Vector3.Transform(new(0, attached.Length * (1 - AttachedOffset), 0), rotMat);
		Loc += new Vector3(rotLoc.X, rotLoc.Y, rotLoc.Z);
		var transform = Transform;
		transform.Origin = Loc;
		Transform = transform;
		Num.Quaternion attachedRot = Num.Quaternion.CreateFromYawPitchRoll(AttachedRotation.Y * (float)Math.PI / 180f, AttachedRotation.X * (float)Math.PI / 180f, AttachedRotation.Z * (float)Math.PI / 180f);
		Rot = Num.Quaternion.Concatenate(attachedRot, Rot);
		Basis = new(new Quaternion(Rot.X, Rot.Y, Rot.Z, Rot.W));
	}
}
