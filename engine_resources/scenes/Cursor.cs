using Godot;
using System;

public partial class Cursor : Node2D
{

    private Sprite2D    VhsCursor   = null;
    private Sprite2D    GameCursor  = null;
    
    private const int FRAME_JETPACK = 1;
    private const int FRAME_EMPTY   = 5;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        VhsCursor   = GetNode<Sprite2D>("%VhsCursor");
        GameCursor  = GetNode<Sprite2D>("%GameCursor");

        Input.MouseMode = Input.MouseModeEnum.Hidden;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        
        this.GlobalPosition = GetGlobalMousePosition();

        if (UiManager.i.UiPeek() != null)
        {
            VhsCursor.Show();
            GameCursor.Hide();
        }
        else if (Global.i.PlayerRef != null)
        {
            VhsCursor.Hide();
            GameCursor.Show();

            if (Global.i.PlayerRef.GetFuelCount() <= 0.0f || !Global.i.PlayerRef.HasJetpack())
            {
                GameCursor.Frame = FRAME_EMPTY;
                return;
            }
            int frameIdx = FRAME_JETPACK;
            if (Input.IsActionPressed("JetpackLeft"))
            {
                frameIdx += 1;
            }
            if (Input.IsActionPressed("JetpackRight"))
            {
                frameIdx += 2;
            }
            GameCursor.Frame = frameIdx;
            return;
        }
        else
        {
            VhsCursor.Hide();
            GameCursor.Hide();
        }
    }
}
