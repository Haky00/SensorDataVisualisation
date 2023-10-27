using Godot;
using System;
using static Postgrest.Constants;

namespace SensorDataVisualisation;

public partial class GuidButton : Button
{
	private PhoneHandler phoneHandler;
	private LineEdit guidEdit;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		guidEdit = GetNode<LineEdit>("../GuidEdit");
		phoneHandler = GetNode<PhoneHandler>("../PhoneContainer/Phone");
		Pressed += OnButtonPress;
	}

	private async void OnButtonPress()
	{
		string content = guidEdit.Text;
        if (!Guid.TryParse(content, out Guid guid))
        {
            return;
        }
        var url = "https://rhtekizmxsrxzkkmhdvc.supabase.co";
		var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InJodGVraXpteHNyeHpra21oZHZjIiwicm9sZSI6ImFub24iLCJpYXQiOjE2OTcyMzQ1NjksImV4cCI6MjAxMjgxMDU2OX0.hl_WAQL84C8LpoqbECkC2o472d3pyjbmjW8n0Pmx03I";
		var dbOptions = new Supabase.SupabaseOptions
		{
			AutoConnectRealtime = false,
		};
		var supabase = new Supabase.Client(url!, key, dbOptions);
		await supabase.InitializeAsync();
		var result = await supabase.From<SensorDataDb>().Filter("Id", Operator.Equals, guid.ToString()).Single();
		var dbData = result;
		phoneHandler.SetData(new(dbData));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
