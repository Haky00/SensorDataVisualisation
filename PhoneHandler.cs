using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Num = System.Numerics;

namespace SensorDataVisualisation;

// This class handles moving the phone according to values gathered from the SensorDataCollecting app
public partial class PhoneHandler : Node3D
{
	private SensorData data;
	public double SimulationSpeed { get; set; } = 1;

	public void Reset()
	{
		Position = new(0, 0, 0);
		Rotation = new(0, 0, 0);
		T = 0;
	}

	// Changes currently used data set
	public void SetData(SensorData newData)
	{
		data = newData;
		Reset();
		ProcessData();
		Reset();
	}

	private List<(double T, Vector3 Pos)> positions;
	private List<(double T, Quaternion Quat)> rotations;

	public bool Paused { get; set; } = true;
	public double TMin { get; set; } = -1;
	public double TMax { get; set; } = -1;
	private double t = 0;
	public double T
	{
		get => t;
		set
		{
			if (value < TMin)
			{
				t = TMin;
			}
			else if (value > TMax)
			{
				t = TMax;
			}
			else
			{
				t = value;
			}
		}
	}

	// Goes through data in LinearAccelerationSensor and RelativeOrientationSensor and pre-calculates phone positions and rotations
	public void ProcessData()
	{
		if (data is null)
		{
			return;
		}
		var orientation = data.RelativeOrientationSensor;
		var accelerometer = data.LinearAccelerationSensor;
		if (orientation is null || accelerometer is null || orientation.Count == 0 || accelerometer.Count == 0)
		{
			return;
		}
		List<(double T, Vector3 Pos)> _pos = new();
		List<(double T, Quaternion Quat)> _rot = new();
		int iO = 0;
		int iA = 0;
		double accelerometerLastT = accelerometer[0].T - 16.6;
		Vector3 velocity = new(0, 0, 0);
		TMin = Math.Max(orientation[0].T, accelerometer[0].T) / 1000;
		TMax = Math.Min(orientation[^1].T, accelerometer[^1].T) / 1000;
		Num.Quaternion additionalRotation = Num.Quaternion.CreateFromAxisAngle(Num.Vector3.UnitX, -90 / 180f * Mathf.Pi);
		while (iO < orientation.Count || iA < accelerometer.Count)
		{
			//GD.Print($"{iO}, {iA}");
			if (iO < orientation.Count && (iA >= accelerometer.Count || orientation[iO].T < accelerometer[iA].T))
			{
				Num.Quaternion rawRotation = new(
					(float)orientation[iO].X,
					(float)orientation[iO].Y,
					(float)orientation[iO].Z,
					(float)orientation[iO].W);
			    // Additional rotation (X -90°) is applied to match the Godot up-direction
				Num.Quaternion rotation = additionalRotation * rawRotation;
				Quaternion quat = new()
				{
					X = rotation.X,
					Y = rotation.Y,
					Z = rotation.Z,
					W = rotation.W
				};
				Basis = new(quat);
				_rot.Add(new(orientation[iO].T / 1000, quat));
				iO++;
			}
			else
			{
				float timescale = (float)(accelerometer[iA].T - accelerometerLastT) / 1000.0f;
				// Damping is applied, as the accelerometer inaccuracies gen create a lot of drift
				velocity.X *= 1f - (0.8f * timescale);
				velocity.Y *= 1f - (0.8f * timescale);
				velocity.Z *= 1f - (0.8f * timescale);
				velocity.X += (float)accelerometer[iA].X * timescale;
				velocity.Y += (float)accelerometer[iA].Y * timescale;
				velocity.Z += (float)accelerometer[iA].Z * timescale;
				Translate(velocity * -timescale);
				_pos.Add((accelerometer[iA].T / 1000, Transform.Origin));
				accelerometerLastT = accelerometer[iA].T;
				iA++;
			}
		}
		T = TMin;
		positions = _pos;
		rotations = _rot;

		// Some metrics about the new data are printed to the godot console
		List<double> upFacingValues = rotations.Select(x => RotationToUpFacingValue(x.Quat, Num.Vector3.UnitZ)).ToList();
		List<double> verticalAlignments = rotations.Select(x => RotationPlaneUpAlignment(x.Quat)).ToList();
		List<double> angleVelocities = rotations.Skip(1).Zip(rotations, (a, b) => RotationsToAngleVelocity(a.Quat, b.Quat, a.T - b.T)).ToList();
		List<double> phoneVelocities = positions.Skip(1).Zip(positions, (a, b) => a.Pos.DistanceTo(b.Pos) / (a.T - b.T)).ToList();
		List<double> phoneAccelerations = accelerometer.Select(a => (double)new Num.Vector3((float)a.X, (float)a.Y, (float)a.Z).Length()).ToList();

		Descriptive accelerationsDesc = new(phoneAccelerations.ToArray());
		accelerationsDesc.Analyze();
		GD.Print("Accelerations Mean: " + accelerationsDesc.Result.Mean);
		GD.Print("Accelerations StdDev: " + accelerationsDesc.Result.StdDev);
		Descriptive upFacingDesc = new(upFacingValues.ToArray());
		upFacingDesc.Analyze();
		GD.Print("Up Facing Mean: " + upFacingDesc.Result.Mean);
		GD.Print("Up Facing StdDev: " + upFacingDesc.Result.StdDev);
		Descriptive verticalAlignmentsDesc = new(verticalAlignments.ToArray());
		verticalAlignmentsDesc.Analyze();
		GD.Print("Vertical Alignments Mean: " + verticalAlignmentsDesc.Result.Mean);
		GD.Print("Vertical Alignments StdDev: " + verticalAlignmentsDesc.Result.StdDev);
		Descriptive angleVelocitiesDesc = new(angleVelocities.ToArray());
		angleVelocitiesDesc.Analyze();
		GD.Print("Angle Velocities Mean: " + angleVelocitiesDesc.Result.Mean);
		GD.Print("Angle Velocities StdDev: " + angleVelocitiesDesc.Result.StdDev);
		Descriptive velocitiesDesc = new(phoneVelocities.ToArray());
		velocitiesDesc.Analyze();
		GD.Print("Velocities Mean: " + velocitiesDesc.Result.Mean);
		GD.Print("Velocities StdDev: " + velocitiesDesc.Result.StdDev);
	}

	// If pre-processed movement data is available, step through it
	public override void _Process(double delta)
	{
		if (positions is null || rotations is null || positions.Count == 0 || rotations.Count == 0)
		{
			return;
		}
		if (!Paused)
		{
			T += delta * SimulationSpeed;
		}
		var rotation = rotations.Where(x => x.T <= T).Aggregate((a, b) => (a.T > b.T) ? a : b);
		var position = positions.Where(x => x.T <= T).Aggregate((a, b) => (a.T > b.T) ? a : b);
		Position = position.Pos;
		Basis = new(rotation.Quat);
	}

	// Functions used for calculating metrics

	private static double RotationToUpFacingValue(Quaternion q, Num.Vector3 forwardDirection)
	{
		Num.Quaternion rotation = new(q.X, q.Y, q.Z, q.W);
		Num.Vector3 forward = Num.Vector3.Transform(forwardDirection, rotation);
		float dotProduct = Num.Vector3.Dot(Num.Vector3.UnitY, forward);
		float angle = MathF.Acos(Math.Clamp(dotProduct, -1, 1));
		angle = MathF.Min(angle, MathF.PI);
		return 1 - angle / MathF.PI;
	}

	private static double RotationsToAngleVelocity(Quaternion q1, Quaternion q2, double deltaTime)
	{
		Num.Quaternion nq1 = new(q1.X, q1.Y, q1.Z, q1.W);
		Num.Quaternion nq2 = new(q2.X, q2.Y, q2.Z, q2.W);
		Num.Quaternion deltaRotation = Num.Quaternion.Inverse(nq1) * nq2;
		double angle = 2 * Math.Acos(Math.Min(1, Math.Abs(deltaRotation.W)));
		return angle / deltaTime;
	}

	private static double RotationPlaneUpAlignment(Quaternion q)
	{
		Num.Quaternion rotation = new(q.X, q.Y, q.Z, q.W);
		Num.Vector3 upVector = Num.Vector3.Transform(Num.Vector3.UnitY, rotation);
		Num.Vector3 forwardVector = Num.Vector3.Transform(Num.Vector3.UnitZ, rotation);
		Num.Vector3 planeNormal = Num.Vector3.Cross(upVector, forwardVector);
		return 1f - 2 * Math.Abs(0.5f - MathF.Acos(Num.Vector3.Dot(planeNormal, Num.Vector3.UnitY) / (forwardVector.Length() * upVector.Length())) / MathF.PI);
	}
}
