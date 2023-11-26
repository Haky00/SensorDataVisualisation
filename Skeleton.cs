using Godot;
using System;
using System.Collections.Generic;

public partial class Skeleton : Node3D
{
	List<BoneScript> bones;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bones = new();
		List<string> boneNames = new(){"Legs", "Torso", "Head", "Shoulder"};
		foreach(string name in boneNames) {
			BoneScript bone = GetNode<BoneScript>(name);
			bones.Add(bone);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach(BoneScript bone in bones) {
			bone.Update();
			bone.UpdateBox();
		}
	}
}
