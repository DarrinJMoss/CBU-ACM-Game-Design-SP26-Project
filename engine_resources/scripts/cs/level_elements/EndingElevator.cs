using Godot;
using System;

public partial class EndingElevator : Area2D
{
    
    [Export] Global.Levels nextLv;

    public async override void _Ready()
    {
        this.BodyEntered += _BodyEntered;
        GetNode<AnimationPlayer>("AnimationTree").Play("RESET");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GetNode<AnimationPlayer>("AnimationTree").Play("init");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void _BodyEntered(Node2D body)
    {
        if (body is Player p)
        {
            p.controlEnabled = false;
            PCam y = p.PCamRef;
            p.CallDeferred("reparent", this);
            y.CallDeferred("reparent", this);
            y.CallDeferred("make_current");
            p.FREEZEE();

            this.SetDeferred("monitoring", false);

            _ = LevelTransitionManager.i.CheckoutTimestamp(nextLv);

            GetNode<AnimationPlayer>("AnimationTree").Play("close");
        }
    }
}
