using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

// Toggles certain nodes to match what is needed for simulation or data display
public partial class SwitchModeButton : Button
{
	private bool SimulationMode = true;

	[Export]
	private Godot.Collections.Array<NodePath> SimulationNodes3D = new();
	[Export]
	private Godot.Collections.Array<NodePath> SimulationNodes2D = new();
	[Export]
	private Godot.Collections.Array<NodePath> DataDisplayNodes3D = new();
	[Export]
	private Godot.Collections.Array<NodePath> DataDisplayNodes2D = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += OnButtonPress;
		SwitchMode(SimulationMode);
	}

	private void OnButtonPress()
	{
		SimulationMode = !SimulationMode;
		SwitchMode(SimulationMode);
	}

	private void SwitchMode(bool mode)
	{
		SetSimulationElementsVisibility(mode);
		SetDataDisplayElementsVisibility(!mode);
		Text = mode ? "Data mode" : "Simulation mode";
	}

	private void SetSimulationElementsVisibility(bool visible)
	{
		foreach(var node in SimulationNodes3D)
		{
			GetNode<Node3D>(node).Visible = visible;
		}
		foreach(var node in SimulationNodes2D)
		{
			GetNode<CanvasItem>(node).Visible = visible;
		}
	}

	private void SetDataDisplayElementsVisibility(bool visible)
	{
		foreach(var node in DataDisplayNodes3D)
		{
			GetNode<Node3D>(node).Visible = visible;
			
		}
		foreach(var node in DataDisplayNodes2D)
		{
			GetNode<CanvasItem>(node).Visible = visible;
		}
	}
}
