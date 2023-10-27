using Godot;
using System;

namespace SensorDataVisualisation;

public abstract class SensorHandler
{
    protected int i = 0;
    protected double lastReadTimestamp = -1;
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
            Quaternion quaternion = new()
            {
                X = (float)data.RelativeOrientationSensor[i].X,
                Y = (float)data.RelativeOrientationSensor[i].Y,
                Z = (float)data.RelativeOrientationSensor[i].Z,
                W = (float)data.RelativeOrientationSensor[i].W
            };
            phone.Basis = new Basis(quaternion);
            lastReadTimestamp = data.RelativeOrientationSensor[i].T;
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
            float timescale = 0.016f;
            if (lastReadTimestamp >= 0)
            {
                timescale = (float)(data.LinearAccelerationSensor[i].T - lastReadTimestamp) / 1000f;
            }
            speed.X *= 1f - (0.8f * (float)timescale);
            speed.Y *= 1f - (0.8f * (float)timescale);
            speed.Z *= 1f - (0.8f * (float)timescale);
            speed.X += (float)data.LinearAccelerationSensor[i].X * timescale;
            speed.Y += (float)data.LinearAccelerationSensor[i].Y * timescale;
            speed.Z += (float)data.LinearAccelerationSensor[i].Z * timescale;
            phone.Translate(speed * -timescale);
            lastReadTimestamp = data.LinearAccelerationSensor[i].T;
            i++;
        }
        //phone.Translate(new(-speed.X * (float)delta, -speed.Y * (float)delta, -speed.Z * (float)delta));
        timestamp += delta * 1000;
    }
}