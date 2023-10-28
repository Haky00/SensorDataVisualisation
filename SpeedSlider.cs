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
		DragEnded += OnDragEnded;
		DragStarted += OnDragStarted;
	}

	private bool dragging = false;

	private void OnDragEnded(bool valueChanged)
	{
		dragging = false;
	}

	private void OnDragStarted()
	{
		dragging = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (dragging)
		{
			phoneHandler.SimulationSpeed = Value;
			speedLabel.Text = $"{Value:N2}";
		}
	}
}
