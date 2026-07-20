using Godot;
using System;

public partial class MusicTesting : Node
{
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.Key0)) {
            MusicManager.i.ChangeTrack(MusicManager.Tracks.SILENT);
        }
        if (Input.IsKeyPressed(Key.Key1)) {
            MusicManager.i.ChangeTrack(MusicManager.Tracks.TITLE);
        }
        if (Input.IsKeyPressed(Key.Key2)) {
            MusicManager.i.ChangeTrack(MusicManager.Tracks.STAGE_1);
        }
        if (Input.IsKeyPressed(Key.Key3)) {
            MusicManager.i.ChangeTrack(MusicManager.Tracks.STAGE_ICE);
        }
    }
}
