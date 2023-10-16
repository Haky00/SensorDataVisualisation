using System;
using System.Collections.Generic;

namespace SensorDataVisualisation;

public class SensorData
{
	public Guid Id { get; set; }
	public MovementType Movement { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
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
