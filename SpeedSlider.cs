using Godot;
using System;

namespace SensorDataVisualisation;

public partial class SpeedSlider : HSlider
{
	private PhoneHandler phoneHandler;
	private Label speedLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		phoneHandler = GetNode<PhoneHandler>("../PhoneContainer/Phone");
		speedLabel = GetNode<Label>("../SpeedLabel");
		DragEnded += OnChange;
	}

	private void OnChange(bool valueChanged)
	{
		if (valueChanged)
		{
			phoneHandler.SimulationSpeed = Value;
			speedLabel.Text = $"{Value:N2}";
			GD.Print(Value);
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
