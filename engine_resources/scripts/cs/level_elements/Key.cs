using Godot;
using System;

public partial class Key : Area2D
{

    public enum KeyColor
    {
        YELLOW,
        RED,
        GREEN,
        BLUE
    }

    [Export] public KeyColor keyColor = KeyColor.YELLOW;

    // If the player's position exceeds the rect defined by these corners, the key is dropped
    [Export] Node2D DropSquareTL = null;
    [Export] Node2D DropSquareBR = null;

    [Export] AudioStreamPlayer2D GetSound;
    [Export] AudioStreamPlayer2D LooseSound;

    [Export] Sprite2D KeySprite;
    [Export] Sprite2D KeySpriteShadow;


    public const float ROTATION_THRESHOLD = 0.01f; 


    Vector2 origin = Vector2.Zero;
    Vector2 targetPos = Vector2.Zero;
    Vector2 curPos = Vector2.Zero;


    float curRot = 0.0f;

    float speed = 0.0f;


    float sineVal = 0.0f;


    public bool isPickedUp = false;
    public bool isDisabled = false;


    public override void _Ready()
    {
        origin = this.GlobalPosition;
        curPos = origin;
        BodyEntered += OnBodyEntered;

        KeySprite.Frame = (int)keyColor;
        KeySpriteShadow.Frame = (int)keyColor;
    }

    public override void _PhysicsProcess(double delta)
    {

        sineVal += (float)delta * 1.5f;
        KeySprite.Scale       = KeySprite.Scale.Lerp      (Vector2.One * 1.0f, Global.i.GetClampedDelta_PH() * 8.0f);
        KeySpriteShadow.Scale = KeySpriteShadow.Scale.Lerp(Vector2.One * 1.0f, Global.i.GetClampedDelta_PH() * 8.0f);

        if (Global.i.PlayerRef != null) {
            if (isPickedUp)
            {
                targetPos = Global.i.PlayerRef.GlobalPosition - new Vector2(25.0f, 25.0f);

                if ((DropSquareBR != null) && (DropSquareTL != null))
                {
                    if ((Global.i.PlayerRef.GlobalPosition.X > DropSquareBR.GlobalPosition.X) || (Global.i.PlayerRef.GlobalPosition.Y > DropSquareBR.GlobalPosition.Y) ||
                        (Global.i.PlayerRef.GlobalPosition.X < DropSquareTL.GlobalPosition.X) || (Global.i.PlayerRef.GlobalPosition.Y < DropSquareTL.GlobalPosition.Y))
                    {
                        Drop();
                    }
                }

            }
            else
            {
                targetPos = origin;
            }
        }

        Vector2 prevPos = curPos;
        curPos = curPos.Lerp(targetPos, 2.5f * Global.i.GetClampedDelta_PH());
        speed = (prevPos - curPos).LengthSquared();
        float movementDir = (curPos - prevPos).Angle();

        if (isPickedUp)
        {
            if (speed > 2.5)
            {
                float thresh = Mathf.Clamp(speed * ROTATION_THRESHOLD, 0.0f, 1.0f);
                curRot = Mathf.LerpAngle(curRot, movementDir, thresh) - ((Mathf.Pi / 2.0f) * thresh);
            }
            else
            {
                curRot = Mathf.LerpAngle(curRot, 0.0f, Global.i.GetClampedDelta_PH() * 5.0f);
            }
            
        } else
        {
            curRot = 0.0f;
        }

        KeySprite.Rotation = curRot + Mathf.Sin(sineVal) * 0.25f;
        KeySpriteShadow.Rotation = curRot + MathF.Sin(sineVal) * 0.25f;

        this.GlobalPosition = curPos + new Vector2(0.0f, Mathf.Cos(sineVal)) * 5.0f;

    }

    public void Drop()
    {
        isPickedUp = false;
        targetPos = origin;
        LooseSound.Play();
    }

    public void Pickup()
    {
        if (!isPickedUp)
        {
            GetSound.Play();
            KeySprite.Scale = Vector2.One * 3.0f;
            KeySpriteShadow.Scale = Vector2.One * 3.0f;
        }
        isPickedUp = true;
    }

    public void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            if (!isPickedUp && !isDisabled)
            {
                Pickup();
            }
        }
    }

}
