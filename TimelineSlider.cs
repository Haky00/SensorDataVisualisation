using Godot;
using System;

namespace SensorDataVisualisation;

public partial class TimelineSlider : HSlider
{
	private PhoneHandler phoneHandler;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		phoneHandler = GetNode<PhoneHandler>("../PhoneContainer/Phone");
		DragEnded += OnDragEnded;
		DragStarted += OnDragStarted;
		Step = 0.01;
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
		if (MinValue != phoneHandler.TMin || MaxValue != phoneHandler.TMax)
		{
			MinValue = phoneHandler.TMin;
			MaxValue = phoneHandler.TMax;
			Value = phoneHandler.T;
		}

		if (dragging)
		{
			phoneHandler.T = Value;
		}
		else
		{
			Value = phoneHandler.T;
		}
	}
}
