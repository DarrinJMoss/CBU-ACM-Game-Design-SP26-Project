using Godot;
using System;

public partial class SpawningElevator : Node2D
{

    private const float LAND_SHAKE_AMNT = 6.0f;
    private const float LAND_SHAKE_DAMP = 350.0f;

    private const float OPEN_SHAKE_AMNT = 1.0f;
    private const float OPEN_SHAKE_DAMP = 350.0f;

    public async override void _Ready()
    {
        Player localPRef = GetNode<Player>("Player");
        localPRef.controlEnabled = false;

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
