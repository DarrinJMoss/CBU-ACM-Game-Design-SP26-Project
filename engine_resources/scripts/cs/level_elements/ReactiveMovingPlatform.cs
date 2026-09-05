using Godot;
using System;

/// <summary>
/// A class that handles a moving platform that only moves when the player stands on it.
/// It then stays at the final position until the player leaves plus a small delay timer.
/// </summary>
public partial class ReactiveMovingPlatform : VariableWall
{
    /// <summary>
    /// The final offset position of the moving platform.
    /// </summary>
    [Export] Vector2 finalPosOffset = Vector2.Up * 50.0f;
    /// <summary>
    /// If set, the interpolation mode uses Mathf.Lerp(). Otherwise it uses Mathf.MoveTowards().
    /// </summary>
    [Export] bool useLerp = true;
    /// <summary>
    /// The speed of the above interpolation (works for both useLerp and !useLerp).
    /// </summary>
    [Export] float interpolationSpeed = 15.0f;

    [Export] float playerDetectionMargin = 3.0f;


    Area2D PlayerDetection;

    Vector2 basePosition = Vector2.Zero;

    bool playerInDetection = false;


    public override void _Ready()
    {
        base._Ready();
        basePosition = this.GlobalPosition;

        PlayerDetection = new Area2D();
        PlayerDetection.CollisionMask = 0b100; // Detect the player only
        this.AddChild(PlayerDetection);
        PlayerDetection.Position = this.Size / 2.0f + new Vector2(0.0f, -playerDetectionMargin / 2.0f);

        CollisionShape2D pdColl = new CollisionShape2D();
        PlayerDetection.AddChild(pdColl);

        RectangleShape2D pdShape = new RectangleShape2D();
        pdShape.Size = this.Size + new Vector2(0.0f, playerDetectionMargin);
        pdColl.Shape = pdShape;

        PlayerDetection.BodyEntered += BodyDetected;
        PlayerDetection.BodyExited  += BodyLeft;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (playerInDetection)
        {
            if (Global.i.PlayerRef.IsOnFloor())
            {
                Move(basePosition + finalPosOffset, (float)delta);
            }
            else
            {
                Move(basePosition, (float)delta);
            }
        }
        else
        {
            Move(basePosition, (float)delta);
        }
    }

    private void BodyDetected(Node2D body)
    {
        if (body is Player)
        {
            playerInDetection = true;
        }
    }
    private void BodyLeft(Node2D body)
    {
        if (body is Player)
        {
            playerInDetection = false;
        }
    }

    private void Move(Vector2 target, float delta)
    {
        Vector2 ogPos = this.GlobalPosition;
        if (useLerp)
        {
            this.GlobalPosition = this.GlobalPosition.Lerp(target, delta * interpolationSpeed);
            if (target != basePosition)
            {
                Global.i.PlayerRef.GlobalPosition += this.GlobalPosition - ogPos;
            }
            
            return;
        }
        this.GlobalPosition = this.GlobalPosition.MoveToward(target, delta * interpolationSpeed);
        if (target != basePosition)
        {
            Global.i.PlayerRef.GlobalPosition += this.GlobalPosition - ogPos;
        }
    }

}
