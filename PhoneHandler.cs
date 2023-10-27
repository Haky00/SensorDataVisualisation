using Godot;
using System;
using System.Collections.Generic;

namespace SensorDataVisualisation;

public partial class PhoneHandler : Node3D
{
	private SensorData data;
	private List<SensorHandler> sensorHandlers = new();
	public double SimulationSpeed { get; set; } = 1;

	// Called when the node enters the scene tree for the first time.
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
	}

	public void SetData(SensorData newData)
	{
		data = newData;
		Reset();
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		foreach (SensorHandler handler in sensorHandlers)
		{
			handler.Update(delta * SimulationSpeed);
		}
	}
}
