using Godot;
using System;

public partial class SpawningElevator : Node2D
{
    [Export] private Node2D BlCamLimtNd = null;
    [Export] private Node2D TrCamLimtNd = null;

    private const float LAND_SHAKE_AMNT = 6.0f;
    private const float LAND_SHAKE_DAMP = 350.0f; 

    private const float OPEN_SHAKE_AMNT = 1.0f;
    private const float OPEN_SHAKE_DAMP = 350.0f;

    public async override void _Ready()
    {
        Player localPRef = GetNode<Player>("Player");
        localPRef.controlEnabled = false;

        Camera2D localCam = GetNode<Camera2D>("Camera2D");
        localCam.Enabled = false;

        localPRef.PCamRef.MakeCurrent();

        localPRef.PCamRef.LimitEnabled  = true;
        localPRef.PCamRef.LimitSmoothed = true;

        localPRef.PCamRef.LimitLeft     = (BlCamLimtNd != null) ? (int)BlCamLimtNd.GlobalPosition.X : -10000000;
        localPRef.PCamRef.LimitBottom   = (BlCamLimtNd != null) ? (int)BlCamLimtNd.GlobalPosition.Y :  10000000;

        localPRef.PCamRef.LimitRight    = (TrCamLimtNd != null) ? (int)TrCamLimtNd.GlobalPosition.X :  10000000;
        localPRef.PCamRef.LimitTop      = (TrCamLimtNd != null) ? (int)TrCamLimtNd.GlobalPosition.Y : -10000000;

        GD.Print(localPRef.PCamRef.LimitTop     );
        GD.Print(localPRef.PCamRef.LimitBottom  );
        GD.Print(localPRef.PCamRef.LimitLeft    );
        GD.Print(localPRef.PCamRef.LimitRight   );
        
        AnimationPlayer a = GetNode<AnimationPlayer>("Anim");
        ColorRect shade = GetNode<ColorRect>("Shade"); shade.Color = Colors.Black;
        
        this.Show();

        localPRef.PCamRef.SetShake(LAND_SHAKE_AMNT, LAND_SHAKE_DAMP);
    
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        a.Play("OpenBegin");

        localPRef.PCamRef.SetShake(OPEN_SHAKE_AMNT, OPEN_SHAKE_DAMP);

        await ToSignal(a, AnimationPlayer.SignalName.AnimationFinished);

        a.Play("OpenFull");

        await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);

        localPRef.controlEnabled = true;

        localPRef.ExitElevator(GetParent());

        foreach (Node n in GetParent().GetChildren())
        {
            GD.Print(n);
            if (n is MusicInterface m && m.deferStart)
            {
                GD.Print("Yeah");
                m.StartMusic();
            }
        }

    }
}
