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

public partial class PhoneHandler : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
		
		string json = File.ReadAllText("data.json");
		data = JsonSerializer.Deserialize<List<SensorData>>(json, options)[0];
		timestamp = data.LinearAccelerationSensor[0].T;
	}

	private int i = 0;
	private double timestamp;
	private SensorData data = new();
    private Vector3 speed = new() { X = 0, Y = 0, Z = 0 };

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
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
			GD.Print(i);
        }

		Translate(new(speed.X * (float)delta, speed.Y * (float)delta, speed.Z * (float)delta));

        timestamp += delta * 1000;
    }
}
