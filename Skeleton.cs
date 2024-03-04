using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Num = System.Numerics;

namespace SensorDataVisualisation;

public partial class Skeleton : Node3D
{
	[Export]
	public bool Animate { get; set; }
	List<BoneScript> bones;
	SkeletonPhoneScript phone;
	List<string> boneNames = new() { "Legs", "Torso", "Head", "Eyes", "Shoulder", "UpperArm", "LowerArm", "Hand" };
	List<BoneParameters> parameters = new();
	public double Speed { get; set; } = 1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bones = new();
		foreach (string name in boneNames)
		{
			BoneScript bone = GetNode<BoneScript>(name);
			bones.Add(bone);
		}
		phone = GetNode<SkeletonPhoneScript>("Phone");
		parameters = JsonConvert.DeserializeObject<List<BoneParameters>>(File.ReadAllText("bestParameters.txt"));
	}

	bool printed = false;
	float time = 0f;
	int n = 0;
	float avg = 0;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		n++;
		time += (float)delta * (float)Speed;
		int param_i = 0;
		foreach (BoneScript bone in bones)
		{
			if (Animate && parameters.Any(p => p.BoneName == bone.Name))
			{
				BoneParameters boneParameters = parameters.First(p => p.BoneName == bone.Name);
				bone.Amount = boneParameters.Amount.Compute(time);
				bone.Angle = boneParameters.Angle.Compute(time);
				bone.Roll = boneParameters.Roll.Compute(time);
			}
			bone.Update();
			bone.UpdateBox();
			param_i++;
		}
		phone.Update();



		if (Animate)
		{
			printed = true;
			Num.Quaternion phoneRotation = phone.Rot;
			Num.Vector3 phonePosition = new(phone.Loc.X, phone.Loc.Y, phone.Loc.Z);
			Num.Vector3 eyesPosition = new(bones[3].Loc.X, bones[3].Loc.Y, bones[3].Loc.Z);
			float distance = Num.Vector3.Distance(phonePosition, eyesPosition);
			Num.Vector3 directionToEyes = Num.Vector3.Normalize(eyesPosition - phonePosition);
			Num.Vector3 phoneForward = Num.Vector3.Transform(Num.Vector3.UnitZ, phoneRotation);
			float facingValue = Num.Vector3.Dot(directionToEyes, phoneForward);
			//GD.Print("Distance between phone and eyes: " + distance);
			GD.Print("Facing value of the phone towards eyes: " + facingValue);
			avg += facingValue;
			GD.Print("Avg. so far: " + (avg / n));
			// GD.Print(phonePosition);
		}
	}
}
