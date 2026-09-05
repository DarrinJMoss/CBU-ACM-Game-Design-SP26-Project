using Godot;
using System;

public partial class LockedGate : VariableWall
{
    /// <summary>
    /// The directions that the gate can take.
    /// </summary>
    enum GateDirections
    {
        HORIZONTAL,
        VERTICAL,
    }

    [ExportCategory("Don't Mess With These")]
    [Export] Sprite2D Lock;
    [Export] Sprite2D Hook;
    [Export] CollisionShape2D   PlayerDetectionHitbox;
    [Export] Area2D             PlayerDetectionArea;

    [Export] Texture2D horizontalTexture;
    [Export] Texture2D verticalTexture;


    [ExportCategory("Mess With These")]
    /// <summary>
    /// The key associated with this door.
    /// </summary>
    [Export] Key partnerKey;
    /// <summary>
    /// The time it takes for the door to close after it is opened.
    /// </summary>
    [Export] float timeToReprime = 5.0f;
    /// <summary>
    /// How large the detection radius is for unlocking the door.
    /// </summary>
    [Export] float unlockDetectionRange = 50.0f;

    [Export] bool allowAirUnlock = false;


    /// <summary>
    /// The orientation of the gate.
    /// </summary>
    GateDirections gateDir = GateDirections.HORIZONTAL;
    /// <summary>
    /// The base size of the gate door. Used for animation code to remember what size
    /// it is supposed to be when closed.
    /// </summary>
    Vector2 baseSize;
    /// <summary>
    /// The target position for the big lock on the door.
    /// </summary>
    Vector2 lockTargetPos;
    /// <summary>
    /// Have we been unlocked?
    /// </summary>
    bool isUnlocked = false;
    /// <summary>
    /// Is the player within the unlock range?
    /// </summary>
    bool playerInUnlockRange = false;
    /// <summary>
    /// The color of the door.
    /// </summary>
    Key.KeyColor color = Key.KeyColor.YELLOW;
    /// <summary>
    /// The timer until the door locks itself again.
    /// </summary>
    float currentTimer = 0.0f;
    float lockShakeAmnt = 0.0f;
    Vector2 hookVel = Vector2.Zero;
    float hookRotVel = 0.0f;


    public override void _Ready()
    {
        
        PlayerDetectionArea.BodyEntered += PlayerDetectorTriggered;
        PlayerDetectionArea.BodyExited  += PlayerDetectorBodyLeft;

        // If there's no partner key, complain and die
        if (partnerKey == null)
        {
            GD.PrintErr("ERR in LockedGate.cs | _Ready() | No partner key assigned to gate.");
            this.QueueFree();
        }
        color = partnerKey.keyColor;

        Lock.Frame = (int)color * 2;
        Hook.Frame = ((int)color * 2) + 1;

        if (this.Size.X >= this.Size.Y)
        {
            gateDir = GateDirections.HORIZONTAL;
            this.Texture = horizontalTexture;
            this.Size = new Vector2(this.Size.X, 8.0f);
        } 
        else
        {
            gateDir = GateDirections.VERTICAL;
            this.Texture = verticalTexture;
            this.Size = new Vector2(8.0f, this.Size.Y);
        }

        // Generate detection hitbox
        Vector2 playerDetectionRect;
        if (gateDir == GateDirections.HORIZONTAL)
        {
            playerDetectionRect = new Vector2(this.Size.X, 2.0f * unlockDetectionRange);
        }
        else
        {
            playerDetectionRect = new Vector2(2.0f * unlockDetectionRange, this.Size.Y);
        }
        PlayerDetectionHitbox.Position = new Vector2(this.Size.X / 2.0f, this.Size.Y / 2.0f);
        RectangleShape2D playerDetectionShape = new RectangleShape2D();
        playerDetectionShape.Size = playerDetectionRect;
        PlayerDetectionHitbox.Shape = playerDetectionShape;

        baseSize = this.Size;

        lockTargetPos = baseSize / 2.0f;
        Lock.Position = lockTargetPos;

        base._Ready();

    }


    public override void _PhysicsProcess(double delta)
    {

        Lock.Position = lockTargetPos + new Vector2((float)GD.RandRange(-lockShakeAmnt, lockShakeAmnt), (float)GD.RandRange(-lockShakeAmnt, lockShakeAmnt));
        lockShakeAmnt = Mathf.Clamp(lockShakeAmnt - (float)delta * 3.0f, 0.0f, 10.0f);

        // Open behavior
        if (isUnlocked)
        {
            Hook.Rotation += hookRotVel * (float)delta;
            Hook.Position += hookVel * (float)delta;
            hookVel.Y += 500.0f * (float)delta;

            // Make the gate close
            if (gateDir == GateDirections.VERTICAL)
            {
                this.Size = new Vector2(this.Size.X, Mathf.Lerp(this.Size.Y, 0.0f, 5.0f * Global.i.GetClampedDelta_PH()));
            }
            else
            {
                this.Size = new Vector2(Mathf.Lerp(this.Size.X, 0.0f, 5.0f * Global.i.GetClampedDelta_PH()), this.Size.Y);
            }
            
            // Reset after timer
            if (currentTimer <= 0.0f)
            {
                // Only close if the player is far enough
                if (!playerInUnlockRange)
                {
                    isUnlocked = false;
                    this.Hitbox.SetDeferred("disabled", false);
                    Lock.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    lockShakeAmnt = 1.5f;
                    Hook.Position = Vector2.Zero;
                    Hook.Rotation = 0.0f;
                    partnerKey.isDisabled = false;
                    partnerKey.Show();
                }
                
            }
            currentTimer -= (float)delta;

            return;
        }

        // Closed behavior

        // Make the gate grow to its closed size.
        if (gateDir == GateDirections.VERTICAL)
        {
            this.Size = new Vector2(this.Size.X, Mathf.Lerp(this.Size.Y, baseSize.Y, 5.0f * Global.i.GetClampedDelta_PH()));
        }
        else
        {
            this.Size = new Vector2(Mathf.Lerp(this.Size.X, baseSize.X, 5.0f * Global.i.GetClampedDelta_PH()), this.Size.Y);
        }

        // Open if all of these are the case:
        //      Our key is picked up
        if (!partnerKey.isPickedUp)
        {
            return;
        }
        //      The player is in range
        if (!playerInUnlockRange)
        {
            return;
        }
        //      The player is on the ground
        if (!Global.i.PlayerRef.IsOnFloor() && !allowAirUnlock)
        {
            return;
        }
        
        // Unlock the door
        isUnlocked = true;
        this.Hitbox.SetDeferred("disabled", true);
        currentTimer = timeToReprime;
        partnerKey.Drop();
        Lock.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        hookVel = new Vector2((float)GD.RandRange(-50.0f, 50.0f), -350.0f);
        hookRotVel = (float)GD.RandRange(-1.8f, 1.8f);
        partnerKey.isDisabled = true;
        partnerKey.Hide();
    }


    private void PlayerDetectorTriggered(Node2D body)
    {
        if (body is Player)
        {
            playerInUnlockRange = true;
        }
    }

    private void PlayerDetectorBodyLeft(Node2D body)
    {
        if (body is Player)
        {
            playerInUnlockRange = false;
        }
    }


}
