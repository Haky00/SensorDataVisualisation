using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SensorDataVisualisation;

public partial class PhoneHandler : Node3D
{
	private SensorData data;
	private List<SensorHandler> sensorHandlers = new();
	public double SimulationSpeed { get; set; } = 1;

	public override void _Ready()
	{

	}

	public void Reset()
	{
		Position = new(0, 0, 0);
		Rotation = new(0, 0, 0);
		sensorHandlers = new() {
			new OrientationHandler(this, data),
			new AccelerationHandler(this, data)
		};
		T = 0;
	}

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
		Vector3 speed = new(0, 0, 0);
		TMin = Math.Max(orientation[0].T, accelerometer[0].T) / 1000;
		TMax = Math.Min(orientation[^1].T, accelerometer[^1].T) / 1000;
		while (iO < orientation.Count || iA < accelerometer.Count)
		{
			//GD.Print($"{iO}, {iA}");
			if (iO < orientation.Count && (iA >= accelerometer.Count || orientation[iO].T < accelerometer[iA].T))
			{
				Quaternion quat = new()
				{
					X = (float)orientation[iO].X,
					Y = (float)orientation[iO].Y,
					Z = (float)orientation[iO].Z,
					W = (float)orientation[iO].W
				};
				Basis = new(quat);
				_rot.Add(new(orientation[iO].T / 1000, quat));
				iO++;
			}
			else
			{
				double timescale = (accelerometer[iA].T - accelerometerLastT) / 1000.0;
				speed.X *= 1f - (0.8f * (float)timescale);
				speed.Y *= 1f - (0.8f * (float)timescale);
				speed.Z *= 1f - (0.8f * (float)timescale);
				speed.X += (float)(accelerometer[iA].X * timescale);
				speed.Y += (float)(accelerometer[iA].Y * timescale);
				speed.Z += (float)(accelerometer[iA].Z * timescale);
				Translate(speed * -(float)timescale);
				_pos.Add((accelerometer[iA].T / 1000, Transform.Origin));
				accelerometerLastT = accelerometer[iA].T;
				iA++;
			}
		}
		T = TMin;
		positions = _pos;
		rotations = _rot;
	}

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
		// GD.Print(rotation);
		// GD.Print(position);	
		Position = position.Pos;
		Basis = new(rotation.Quat);

		// foreach (SensorHandler handler in sensorHandlers)
		// {
		// 	handler.Update(delta * SimulationSpeed);
		// }
	}
}
