using Godot;
using System;
using System.Collections.Generic;

namespace SensorDataVisualisation;

public partial class Skeleton : Node3D
{
	[Export]
	public bool Animate { get; set; }
	List<BoneScript> bones;
	SkeletonPhoneScript phone;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bones = new();
		List<string> boneNames = new() { "Legs", "Torso", "Head", "Shoulder", "UpperArm", "LowerArm", "Hand" };
		foreach (string name in boneNames)
		{
			BoneScript bone = GetNode<BoneScript>(name);
			bones.Add(bone);
		}
		phone = GetNode<SkeletonPhoneScript>("Phone");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (BoneScript bone in bones)
		{
			if (Animate)
			{
				bone.Animate(delta);
			}
			bone.Update();
			bone.UpdateBox();
		}
		phone.Update();
	}
}
