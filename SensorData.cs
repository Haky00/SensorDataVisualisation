using System;
using System.Collections.Generic;
using System.Text.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace SensorDataVisualisation;

public class SensorDataDb : BaseModel
{
	[PrimaryKey]
	public Guid Id { get; set; }
	[Column]
	public MovementType Movement { get; set; }
	[Column]
	public DateTime StartTime { get; set; }
	[Column]
	public DateTime EndTime { get; set; }
	[Column]
	public DateTime UploadTime { get; set; }
	[Column]
	public Gender? Gender { get; set; }
	[Column]
	public string Phone { get; set; }
	[Column]
	public int? Age { get; set; }
	[Column]
	public int? Weight { get; set; }
	[Column]
	public int? Height { get; set; }
	[Column]
	public string Accelerometer { get; set; }
	[Column]
	public string GravitySensor { get; set; }
	[Column]
	public string Gyroscope { get; set; }
	[Column]
	public string LinearAccelerationSensor { get; set; }
	[Column]
	public string AbsoluteOrientationSensor { get; set; }
	[Column]
	public string RelativeOrientationSensor { get; set; }
}

public class SensorData
{
	public Guid Id { get; set; }
	public MovementType Movement { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public DateTime UploadTime { get; set; }
	public Gender? Gender { get; set; }
	public string Phone { get; set; }
	public int? Age { get; set; }
	public int? Weight { get; set; }
	public int? Height { get; set; }
	public List<SensorXYZ> Accelerometer { get; set; }
	public List<SensorXYZ> GravitySensor { get; set; }
	public List<SensorXYZ> Gyroscope { get; set; }
	public List<SensorXYZ> LinearAccelerationSensor { get; set; }
	public List<SensorXYZW> AbsoluteOrientationSensor { get; set; }
	public List<SensorXYZW> RelativeOrientationSensor { get; set; }

	public SensorData(SensorDataDb db) {
		Id = db.Id;
		Movement = db.Movement;
		StartTime = db.StartTime;
		EndTime = db.EndTime;
		UploadTime = db.UploadTime;
		Gender = db.Gender;
		Phone = db.Phone;
		Age = db.Age;
		Weight = db.Weight;
		Height = db.Height;
		Accelerometer = JsonSerializer.Deserialize<List<SensorXYZ>>(db.Accelerometer);
		GravitySensor = JsonSerializer.Deserialize<List<SensorXYZ>>(db.GravitySensor);
		Gyroscope = JsonSerializer.Deserialize<List<SensorXYZ>>(db.Gyroscope);
		LinearAccelerationSensor = JsonSerializer.Deserialize<List<SensorXYZ>>(db.LinearAccelerationSensor);
		AbsoluteOrientationSensor = JsonSerializer.Deserialize<List<SensorXYZW>>(db.AbsoluteOrientationSensor);
		RelativeOrientationSensor = JsonSerializer.Deserialize<List<SensorXYZW>>(db.RelativeOrientationSensor);
	}
}

public struct SensorXYZ
{
	public double T { get; set; }
	public double X { get; set; }
	public double Y { get; set; }
	public double Z { get; set; }
}

public struct SensorXYZW
{
	public double T { get; set; }
	public double X { get; set; }
	public double Y { get; set; }
	public double Z { get; set; }
	public double W { get; set; }
}

public enum Gender
{
	Male,
	Female,
	Other,
}

public enum MovementType
{
	Lay,
	Sit,
	Walk,
	Run,
	StairsDown,
	StairsUp,
	Car,
	Bus,
	Tram,
	Train,
	Metro,
	OnTable
}
