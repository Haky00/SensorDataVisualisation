using Godot;
using System;

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
	public float MaxAngle { get; set; }
	[Export]
	public Vector3 AngleFactor { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CsgBox3D box = new() {
			Size = new(0.1f, Length, 0.1f),
		};
		box.Transform.Translated(new Vector3(0, Length * (0.5f) + Length * (1 - AttachedOffset), 0));
		AddChild(box);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
