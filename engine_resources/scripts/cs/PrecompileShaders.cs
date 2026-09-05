using Godot;
using System;
using System.Threading.Tasks;

public partial class PrecompileShaders : Node
{

    public override async void _Ready()
    {
        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);

        GetTree().ChangeSceneToFile("res://engine_resources/scenes/splashes.tscn");
    }


}
