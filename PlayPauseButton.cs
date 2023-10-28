using Godot;
using System;

namespace SensorDataVisualisation;

public partial class PlayPauseButton : Button
{
	private PhoneHandler phoneHandler;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		phoneHandler = GetNode<PhoneHandler>("../PhoneContainer/Phone");
		Pressed += OnButtonPress;
	}

	private void OnButtonPress()
	{
		phoneHandler.Paused = !phoneHandler.Paused;
		Text = phoneHandler.Paused ? "Play" : "Pause";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
