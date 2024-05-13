using Godot;
using System;
using static Postgrest.Constants;

namespace SensorDataVisualisation;

// This controls the "OK" button next to the GUID field in data mode
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
		// This is a public facing key, so it is fine to just use as plaintext
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
}
