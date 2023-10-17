using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorDataVisualisation;

public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		Debug.Assert(typeToConvert == typeof(DateTime));
		return DateTime.Parse(reader.GetString() ?? string.Empty);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}

public abstract class SensorHandler
{
	protected int i = 0;
	protected double timestamp;
	protected Node3D phone;
	protected SensorData data;
	public SensorHandler(Node3D phone, SensorData data)
	{
		this.phone = phone;
		this.data = data;
	}
	public abstract void Update(double delta);
}

public class OrientationHandler : SensorHandler
{
	public OrientationHandler(Node3D phone, SensorData data) : base(phone, data)
	{
		if (data.RelativeOrientationSensor.Count > 0)
		{
			timestamp = data.RelativeOrientationSensor[0].T;
		}
	}

	public override void Update(double delta)
	{
		if (i >= data.RelativeOrientationSensor.Count)
		{
			return;
		}
		if (timestamp >= data.RelativeOrientationSensor[i].T)
		{
			Quaternion quaternion = new(
				(float)data.RelativeOrientationSensor[i].X,
				(float)data.RelativeOrientationSensor[i].Y,
				(float)data.RelativeOrientationSensor[i].Z,
				(float)data.RelativeOrientationSensor[i].W
			);
			Vector3 rotation = quaternion.GetEuler(EulerOrder.Xyz);
			phone.Rotation = rotation;
			GD.Print(rotation);

			// reset rotation
			//Transform3D transform = phone.Transform;
			//transform.Basis = Basis.Identity;
			//phone.Transform = transform;

			//phone.RotateObjectLocal(Vector3.Up, rotation.X); // first rotate about Y
			//phone.RotateObjectLocal(Vector3.Right, rotation.Y); // then rotate about X
			//phone.RotateObjectLocal(Vector3.Back, rotation.Z); // then rotate about Z
			i++;
		}
		timestamp += delta * 1000;
	}
}

public class AccelerationHandler : SensorHandler
{
	private Vector3 speed = new() { X = 0, Y = 0, Z = 0 };
	public AccelerationHandler(Node3D phone, SensorData data) : base(phone, data)
	{
		if (data.LinearAccelerationSensor.Count > 0)
		{
			timestamp = data.LinearAccelerationSensor[0].T;
		}
	}
	public override void Update(double delta)
	{
		if (i >= data.LinearAccelerationSensor.Count)
		{
			return;
		}
		if (timestamp >= data.LinearAccelerationSensor[i].T)
		{
			speed.X = (float)data.LinearAccelerationSensor[i].X;
			speed.Y = (float)data.LinearAccelerationSensor[i].Y;
			speed.Z = (float)data.LinearAccelerationSensor[i].Z;
			i++;
		}
		phone.Translate(new(speed.X * (float)delta, speed.Y * (float)delta, speed.Z * (float)delta));
		timestamp += delta * 1000;
	}
}

public partial class PhoneHandler : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

		string json = File.ReadAllText("data.json");
		data = JsonSerializer.Deserialize<List<SensorData>>(json, options)[0];
		sensorHandlers.Add(new OrientationHandler(this, data));
		sensorHandlers.Add(new AccelerationHandler(this, data));
	}
	private SensorData data = new();
	private List<SensorHandler> sensorHandlers = new();


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (SensorHandler handler in sensorHandlers)
		{
			handler.Update(delta);
		}
	}
}
