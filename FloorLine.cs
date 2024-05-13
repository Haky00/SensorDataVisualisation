using Godot;
using System;

// Controls the line (box) which is drawn from the phone to the floor
public partial class FloorLine : CsgBox3D
{
	// Called when the node enters the scene tree for the first time.
	Node3D phone;
	Node3D phoneContainer;
	Node3D floor;
	public override void _Ready()
	{
		phone = GetNode<Node3D>("../PhoneContainer/Phone");
		phoneContainer = GetNode<Node3D>("../PhoneContainer");
		floor = GetNode<Node3D>("../Floor");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float height = phone.Position.Y + phoneContainer.Position.Y - floor.Position.Y - 0.085f;
		float x = phone.Position.X + phoneContainer.Position.X;
		float y = floor.Position.Y + height / 2;
		float z = phone.Position.Z + phoneContainer.Position.Z;
		Position = new Vector3(x, y ,z);
		Size = new Vector3(Size.X, height, Size.Z);
	}
}
