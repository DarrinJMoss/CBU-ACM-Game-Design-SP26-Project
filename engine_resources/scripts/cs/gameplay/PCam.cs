using Godot;
using System;
using System.Data;

public partial class PCam : Camera2D
{
    const float LAG_PAN_RECOVERY_RATE = 0.0f;
    const float LAG_PAN_BONUS_INCRATE = 5.0f;
    const float LAG_PAN_STRENGTH = 1.0f;

    const float DEFAULT_SHAKE_DAMPING = 1.0f; // Not a delta time value, runs at a fixed rate based on shake delta
    const float SHAKE_DELTA = 1.0f / 60.0f;   // Camera shake locked to 30 fps

    const float PAN_STRENGTH = 1.5f;

    const float DEFAULT_ZOOM = 1.1f;
    
    private Vector2 panVelocity         = Vector2.Zero;
    private Vector2 panTargetVelocity   = Vector2.Zero;
    private Vector2 panAngle            = Vector2.Zero;
    private Vector2 panTargetAngle      = Vector2.Zero;
    private Vector2 lagAmount           = Vector2.Zero;
    private Vector2 shakeAmount         = Vector2.Zero;

    private float lagTimer = 0.0f;
    private float lagTimerMax = 0.1f;
    private float lagPanBonus = 0.0f;

    private float curShake = 0.0f;
    private float shakeTimer = 0.0f;
    private float shakeDamping = 1.0f;

    public override void _Ready()
    {
        this.Zoom = Vector2.One * DEFAULT_ZOOM;
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Global.i.PlayerRef == null)
        {
            return;
        }
        panAngle    = panAngle.Lerp   (panTargetAngle,    PAN_STRENGTH * Global.i.GetClampedDelta_PH());
        panVelocity = panVelocity.Lerp(panTargetVelocity, PAN_STRENGTH * Global.i.GetClampedDelta_PH());

        if (lagTimer > 0.0f)
        {
            lagPanBonus = 0.0f;
            Vector2 prevPos = this.GlobalPosition;
            this.GlobalPosition = Global.i.PlayerRef.GlobalPosition;
            lagAmount -= (this.GlobalPosition - prevPos) * (LAG_PAN_STRENGTH * (lagTimer / lagTimerMax));
            lagTimer -= (float)delta;
        } 
        else
        {
            lagPanBonus += (float)delta * LAG_PAN_BONUS_INCRATE;
            lagAmount = lagAmount.Lerp(Vector2.Zero, (LAG_PAN_RECOVERY_RATE + lagPanBonus) * Global.i.GetClampedDelta_PH());
            this.GlobalPosition = Global.i.PlayerRef.GlobalPosition;
        }

        shakeTimer -= (float)delta;
        if (shakeTimer <= 0.0f)
        {
            shakeAmount = new Vector2((float)GD.RandRange(-curShake, curShake), (float)GD.RandRange(-curShake, curShake));
            curShake = Mathf.Clamp(curShake - (float)delta * shakeDamping * Mathf.Max(SHAKE_DELTA, (float)delta), 0.0f, 9999999.0f);
            shakeTimer = SHAKE_DELTA;
        }
        

        this.Offset = panAngle + panVelocity + lagAmount + shakeAmount;
    }

    public void Lag(float time)
    {
        if (time <= 0.0f) // This would be a divide by zero, so just a safety thing.
        {
            return;
        }
        lagTimerMax = time;
        lagTimer = time;
    }

    public void SetVelocityPan(Vector2 nPan)
    {
        panTargetVelocity = nPan;
    }
    public void SetAnglePan(Vector2 nPan)
    {
        panTargetAngle = nPan;
    }

    public void SetShake(float amount, float damping = DEFAULT_SHAKE_DAMPING)
    {
        shakeTimer = 0.0f;
        curShake = Mathf.Max(curShake, amount);
        shakeDamping = Mathf.Max(shakeDamping, damping);
    }

}
