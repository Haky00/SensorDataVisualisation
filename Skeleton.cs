using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Num = System.Numerics;

namespace SensorDataVisualisation;

// This class controls both simulating the whole skeleton and operations related to parameters
// It may be a good idea to split it into two (Skeleton and SkeletonController or similar)
public partial class Skeleton : Node3D
{
	private bool animate;
	// If false, the skeleton is not simulated
	[Export]
	public bool Animate
	{
		get => animate;
		set
		{
			animate = value;
			if (animateButton is not null)
			{
				animateButton.ButtonPressed = animate;
			}
		}
	}

	private bool constantWalk;
	// Enables or disables the constant values of leg direction, as well as direction
	[Export]
	public bool ConstantWalk
	{
		get => constantWalk;
		set
		{
			constantWalk = value;
			if (constantWalkButton is not null)
			{
				constantWalkButton.ButtonPressed = constantWalk;
			}
		}
	}

	List<BoneScript> bones;
	LegScript legs;
	ItemList parametersList;
	SkeletonPhoneScript phone;
	static readonly List<string> boneNames = new() { "Torso", "Head", "Eyes", "Shoulder", "Upper Arm", "Lower Arm", "Hand" };
	SimulationParameters parameters;
	SimulationParameters lastParameters;
	double parametersSwitchTime = parameterSwitchTimeMax;
	const double parameterSwitchTimeMax = 3;
	public double Speed { get; set; } = 1;

	private CheckButton animateButton;
	private CheckButton constantWalkButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		legs = GetNode<LegScript>("Legs");
		bones = new();
		foreach (string name in boneNames)
		{
			BoneScript bone = GetNode<BoneScript>(name);
			bones.Add(bone);
		}
		phone = GetNode<SkeletonPhoneScript>("Phone");
		parametersList = GetNode<ItemList>("../ParametersList");
		GetNode<Button>("../RefreshParameters").Pressed += UpdateParametersList;
		GetNode<Button>("../DeleteParameters").Pressed += DeleteSelectedParameters;
		GetNode<Button>("../UseParameters").Pressed += UseSelectedParameters;
		GetNode<Button>("../ExportButton").Pressed += ExportAllParameters;
		GetNode<Button>("../ExportCurrentButton").Pressed += ExportCurrentParameters;
		GetNode<Button>("../ResetPositionButton").Pressed += ResetPosition;
		GetNode<CheckButton>("../AnimateButton").Pressed += () => { Animate = !Animate; };
		GetNode<CheckButton>("../ConstantWalkButton").Pressed += () => { ConstantWalk = !ConstantWalk; };

		Animate = animate;
		ConstantWalk = constantWalk;

		UpdateParametersList();
	}

	private void UpdateParametersList()
	{
		parametersList.Clear();
		DirectoryInfo parametersFolder = new("Parameters");
		var parametersFiles = parametersFolder.GetFiles();
		Array.Sort(parametersFiles, (f1, f2) => f1.Name.CompareTo(f2.Name));
		foreach (FileInfo parametersFile in parametersFiles)
		{
			parametersList.AddItem(parametersFile.Name);
		}
	}

	private string ParametersListSelectedName()
	{
		int[] selectedIndexes = parametersList.GetSelectedItems();
		if (selectedIndexes.Length == 0)
		{
			return null;
		}
		return parametersList.GetItemText(selectedIndexes[0]);
	}

	private void DeleteSelectedParameters()
	{
		string selectedName = ParametersListSelectedName();
		if (selectedName == null)
		{
			return;
		}
		File.Delete("Parameters/" + selectedName);
		UpdateParametersList();
	}

	private void UseSelectedParameters()
	{
		string selectedName = ParametersListSelectedName();
		if (selectedName == null)
		{
			return;
		}
		lastParameters = parameters;
		parameters = JsonConvert.DeserializeObject<SimulationParameters>(File.ReadAllText("Parameters/" + selectedName));
		parametersSwitchTime = parameterSwitchTimeMax;
	}

	private static void ExportParameters(List<SimulationParameters> parameters)
	{
		File.WriteAllText("wrappingL-SENSOR-DATA.js", "var skeleton_parameters_json = `" + JsonConvert.SerializeObject(parameters, Formatting.Indented) + "`;");
	}

	private void ExportAllParameters()
	{
		List<SimulationParameters> parameters = new();
		DirectoryInfo parametersFolder = new("Parameters");
		var parametersFiles = parametersFolder.GetFiles();
		Array.Sort(parametersFiles, (f1, f2) => f1.Name.CompareTo(f2.Name));
		foreach (FileInfo parametersFile in parametersFiles)
		{
			parameters.Add(JsonConvert.DeserializeObject<SimulationParameters>(File.ReadAllText(parametersFile.FullName)));
		}
		ExportParameters(parameters);
	}

	private void ExportCurrentParameters()
	{
		ExportParameters(new() { parameters });
	}

	private void ResetPosition()
	{
		legs.Loc = Vector3.Zero;
	}

	float time = 0f;

	static readonly Vector2 locationLoop = new(8, 8);

	private static float InterpolateValue(float v1, float v2, float time) => v1 * (1f - time) + v2 * time;
	private float InterpolateParameters(float? v1, float? v2)
	{
		double switchPortion = (parameterSwitchTimeMax - parametersSwitchTime) / parameterSwitchTimeMax;
		return InterpolateValue(v1 ?? 0f, v2 ?? 0f, Smoothstep(MathF.Min((float)switchPortion, 1f)));
	}

	public static float Smoothstep(float t) => t * t * t * (t * (t * 6 - 15) + 10);

	// float avg = 0;
	// private Num.Quaternion lastRotation = Num.Quaternion.Identity;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!animate)
		{
			delta = 0;
		}
		float scaledTime = (float)delta * (float)Speed;
		time += scaledTime;
		parametersSwitchTime -= scaledTime;
		int param_i = 0;
		legs.Velocity = new(
			InterpolateParameters(lastParameters?.Legs.VelocityX.Compute(time), parameters?.Legs.VelocityX.Compute(time)),
			InterpolateParameters(lastParameters?.Legs.VelocityY.Compute(time), parameters?.Legs.VelocityY.Compute(time)),
			InterpolateParameters(lastParameters?.Legs.VelocityZ.Compute(time), parameters?.Legs.VelocityZ.Compute(time)));
		if (!ConstantWalk)
		{
			legs.Velocity -= new Vector3(
				InterpolateParameters(lastParameters?.Legs.VelocityX.Constant, parameters?.Legs.VelocityX.Constant),
				InterpolateParameters(lastParameters?.Legs.VelocityY.Constant, parameters?.Legs.VelocityY.Constant),
				InterpolateParameters(lastParameters?.Legs.VelocityZ.Constant, parameters?.Legs.VelocityZ.Constant));
		}
		if (ConstantWalk)
		{
			legs.Direction = InterpolateParameters(lastParameters?.Legs.Direction.Compute(time), parameters?.Legs.Direction.Compute(time));
		}
		legs.Update(scaledTime);
		legs.UpdateBox();
		foreach (BoneScript bone in bones)
		{
			if (Animate)
			{
				BoneParameters? lastBoneParameters = lastParameters?.Bones.FirstOrDefault(p => p.BoneName == bone.Name);
				BoneParameters? boneParameters = parameters?.Bones.FirstOrDefault(p => p.BoneName == bone.Name);
				bone.Amount = InterpolateParameters(lastBoneParameters?.Amount.Compute(time), boneParameters?.Amount.Compute(time));
				bone.Angle = InterpolateParameters(lastBoneParameters?.Angle.Compute(time), boneParameters?.Angle.Compute(time));
				bone.Roll = InterpolateParameters(lastBoneParameters?.Roll.Compute(time), boneParameters?.Roll.Compute(time));
			}
			bone.Update();
			bone.UpdateBox();
			param_i++;
		}
		phone.Update();

		int xLoops = (int)Math.Floor((legs.Loc.X + locationLoop.X / 2) / locationLoop.X);
		int zLoops = (int)Math.Floor((legs.Loc.Z + locationLoop.Y / 2) / locationLoop.Y);
		var transform = Transform;
		transform.Origin = new(-xLoops * locationLoop.X, 0, -zLoops * locationLoop.Y);
		Transform = transform;

		Num.Quaternion phoneRotation = phone.Rot;
		Num.Vector3 phonePosition = new(phone.Loc.X, phone.Loc.Y, phone.Loc.Z);
		Num.Vector3 eyesPosition = new(bones[3].Loc.X, bones[3].Loc.Y, bones[3].Loc.Z);
		float distance = Num.Vector3.Distance(phonePosition, eyesPosition);
		Num.Vector3 directionToEyes = Num.Vector3.Normalize(eyesPosition - phonePosition);
		Num.Vector3 phoneForward = Num.Vector3.Transform(Num.Vector3.UnitZ, phoneRotation);
		float facingValue = Num.Vector3.Dot(directionToEyes, phoneForward);

		// Previously used statements for debugging, leaving this in for now

		// GD.Print("Distance between phone and eyes: " + distance);
		// GD.Print("Facing value of the phone towards eyes: " + facingValue);
		// avg += facingValue;
		// GD.Print("Avg. so far: " + (avg / n));
		// GD.Print(phonePosition);
		// GD.Print(RotationToUpFacingValue(phoneRotation));
		// GD.Print(RotationToUpFacingAngle(phoneRotation));
		// GD.Print(RotationToUpFacingAngleTop(phoneRotation));
		// GD.Print(RotationPlaneUpAlignment(phoneRotation));
		// if (lastRotation != phoneRotation)
		// {
		// 	GD.Print(Gyro(lastRotation, phoneRotation, scaledTime));
		// 	lastRotation = phoneRotation;
		// }
	}

	// private static Num.Vector3 Gyro(Num.Quaternion q1, Num.Quaternion q2, float delta)
	// {
	// 	Num.Quaternion relativeRotation = Num.Quaternion.Inverse(q1) * q2;
	// 	return ToEulerAngles(relativeRotation) / delta;
	// }

	// private static Num.Vector3 ToEulerAngles(Num.Quaternion q)
	// {
	// 	double yaw, pitch, roll;
	// 	double test = q.X * q.Y + q.Z * q.W;
	// 	if (test > 0.499)
	// 	{ 
	// 		yaw = 2 * Math.Atan2(q.X, q.W);
	// 		pitch = Math.PI / 2;
	// 		roll = 0;
	// 		return new((float)roll, (float)yaw, (float)pitch);
	// 	}
	// 	if (test < -0.499)
	// 	{ 
	// 		yaw = -2 * Math.Atan2(q.X, q.W);
	// 		pitch = -Math.PI / 2;
	// 		roll = 0;
	// 		return new((float)roll, (float)yaw, (float)pitch);
	// 	}
	// 	double sqx = q.X * q.X;
	// 	double sqy = q.Y * q.Y;
	// 	double sqz = q.Z * q.Z;
	// 	yaw = Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * sqy - 2 * sqz);
	// 	pitch = Math.Asin(2 * test);
	// 	roll = Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, 1 - 2 * sqx - 2 * sqz);

	// 	return new((float)roll, (float)yaw, (float)pitch);
	// }

	// private static float RotationToUpFacingValue(Num.Quaternion rotation)
	// {
	// 	Num.Vector3 forward = Num.Vector3.Transform(Num.Vector3.UnitZ, rotation);
	// 	float dotProduct = Num.Vector3.Dot(Num.Vector3.UnitY, forward);
	// 	float angle = MathF.Acos(Math.Clamp(dotProduct, -1, 1));
	// 	angle = MathF.Min(angle, MathF.PI);
	// 	return 1 - angle / MathF.PI;
	// }

	// private static float RotationToUpFacingAngle(Num.Quaternion rotation)
	// {
	// 	Num.Vector3 forward = Num.Vector3.Transform(Num.Vector3.UnitZ, rotation);
	// 	float dotProduct = Num.Vector3.Dot(Num.Vector3.UnitY, forward);
	// 	float angle = MathF.Acos(Math.Clamp(dotProduct, -1, 1));
	// 	angle = MathF.Min(angle, MathF.PI);
	// 	return angle * (180f / MathF.PI);
	// }

	// private static float RotationToUpFacingAngleTop(Num.Quaternion rotation)
	// {
	// 	Num.Vector3 forward = Num.Vector3.Transform(Num.Vector3.UnitY, rotation);
	// 	float dotProduct = Num.Vector3.Dot(Num.Vector3.UnitY, forward);
	// 	float angle = MathF.Acos(Math.Clamp(dotProduct, -1, 1));
	// 	angle = MathF.Min(angle, MathF.PI);
	// 	return angle * (180f / MathF.PI);
	// }

	// private static float RotationPlaneUpAlignment(Num.Quaternion rotation)
	// {
	// 	Num.Vector3 upVector = Num.Vector3.Transform(Num.Vector3.UnitY, rotation);
	// 	Num.Vector3 forwardVector = Num.Vector3.Transform(Num.Vector3.UnitZ, rotation);
	// 	Num.Vector3 planeNormal = Num.Vector3.Cross(upVector, forwardVector);
	// 	return 1f - 2 * Math.Abs(0.5f - MathF.Acos(Num.Vector3.Dot(planeNormal, Num.Vector3.UnitY) / (forwardVector.Length() * upVector.Length())) / MathF.PI);
	// }
}
