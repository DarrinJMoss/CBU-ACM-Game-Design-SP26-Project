using Godot;
using System;

[GlobalClass]
public partial class MusicInterface : Node
{
    [Export]
    public MusicManager.Tracks      curTrack         = MusicManager.Tracks.SILENT;

    private int                     failureCount    = 0;

    public override void _Ready()
    {
        if (MusicManager.i == null)
        {
            // Initialization failed
            Global.LogWarning("MusicInterface._Process()", "engine_resources/scripts/cs/MusicInterface.cs", $"Could not change track due to MusicManager instance being null. Attempting to change it on the following frame.");
            return;
        }
        MusicManager.i.ChangeTrack(curTrack);
        this.QueueFree();
        this.SetProcess(false);
    }

    // If initialization fails, we try again every frame until it works.
    // If it doesn't after a few, it never will and we'll need to annoy
    // the developer until they fix it.
    public override void _Process(double delta)
    {
        if (MusicManager.i == null)
        {
            failureCount++;
            Global.LogError("MusicInterface._Process()", "engine_resources/scripts/cs/MusicInterface.cs", $"Could not change track due to MusicManager instance being null after defaulting to process changing. Failure Count: {failureCount}");
            return;
        }
        MusicManager.i.ChangeTrack(curTrack);
        this.QueueFree();
        this.SetProcess(false);
    }

}
